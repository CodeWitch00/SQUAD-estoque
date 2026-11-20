"""Synthetic probes only. Run with Python 3.10+, .NET 10 and Git; optional Docker.

Proves exclusion from Git, dotnet publish and (with --docker) Docker context.
Does not scan real production secrets or change a shared environment.
"""
import argparse
import pathlib
import subprocess
import tarfile
import tempfile
import uuid


def run(args, **kwargs):
    return subprocess.run(args, check=True, **kwargs)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--docker', action='store_true')
    args = parser.parse_args()
    root = pathlib.Path(__file__).resolve().parents[1]
    project = root / 'src/SquadEstoque.Web'
    marker = ('SQUAD_SYNTHETIC_PROBE_' + uuid.uuid4().hex).encode()
    probes = ['.env', '.env.production', 'appsettings.Secrets.json',
              'appsettings.Production.local.json', 'certificate.pfx',
              'certificate.p12', 'certificate.pem', 'private.key',
              'secrets/value.txt', 'keys/key.xml', 'wwwroot/.env',
              'wwwroot/appsettings.Secrets.json']
    created = []
    created_dirs = []
    tag = 'squad-secret-probe:' + uuid.uuid4().hex
    container = None
    image_built = False
    try:
        # Exclusive create: never overwrite an operator's local configuration.
        for name in probes:
            path = project / name
            if not path.parent.exists():
                path.parent.mkdir()
                created_dirs.append(path.parent)
            with path.open('xb') as stream:
                created.append(path)
                stream.write(b'{"SyntheticProbe":"' + marker + b'"}')
            relative = path.relative_to(root).as_posix()
            tracked = run(['git', 'ls-files', '--', relative], cwd=root,
                          capture_output=True).stdout
            if tracked:
                raise RuntimeError('Sensitive probe path is already tracked: ' + relative)
            run(['git', 'check-ignore', '-q', '--', relative], cwd=root)
        print('PASS: all synthetic sensitive paths are ignored by Git', flush=True)
        with tempfile.TemporaryDirectory(prefix='squad-publish-probe-') as target:
            run(['dotnet', 'publish', str(project / 'SquadEstoque.Web.csproj'),
                 '-c', 'Release', '-o', target], cwd=root)
            for path in pathlib.Path(target).rglob('*'):
                if path.is_file() and marker in path.read_bytes():
                    raise RuntimeError('Synthetic secret reached publish output')
            if not (pathlib.Path(target) / 'SquadEstoque.Web.dll').is_file():
                raise RuntimeError('Publish output missing application')
            print('PASS: publish excludes every synthetic value', flush=True)
        if args.docker:
            # Real Docker ignore semantics, including nested paths. No network image needed.
            run(['docker', 'build', '-t', tag, '-f', '-', '.'], cwd=root,
                input=b'FROM scratch\nCOPY . /context/\n')
            image_built = True
            container = run(['docker', 'create', tag, '/unused'],
                            capture_output=True, text=True).stdout.strip()
            with tempfile.TemporaryDirectory(prefix='squad-context-probe-') as target:
                archive = pathlib.Path(target) / 'context.tar'
                run(['docker', 'export', '-o', str(archive), container])
                with tarfile.open(archive) as tar:
                    for entry in tar:
                        if entry.isfile():
                            with tar.extractfile(entry) as stream:
                                if marker in stream.read():
                                    raise RuntimeError('Synthetic secret reached Docker context')
            print('PASS: Docker context excludes every synthetic value', flush=True)
        else:
            print('NOT RUN: Docker context (use --docker)', flush=True)
    finally:
        if container:
            subprocess.run(['docker', 'rm', container], check=False, capture_output=True)
        if image_built:
            subprocess.run(['docker', 'image', 'rm', tag], check=False, capture_output=True)
        for path in reversed(created):
            path.unlink()
        for path in reversed(created_dirs):
            path.rmdir()


if __name__ == '__main__':
    main()
