"""Gera o diagrama UML de casos de uso do SQUAD Estoque em SVG."""
from pathlib import Path
from xml.sax.saxutils import escape

W, H = 605, 790
shapes = []
def add(s): shapes.append(s)
def line(points):
    add('<polyline points="%s" fill="none" stroke="#536275" stroke-width="1.15" stroke-linejoin="round"/>' % ' '.join(f'{x},{y}' for x,y in points))
def label(x,y,lines,size=11.8,weight='normal'):
    lines = lines if isinstance(lines,list) else [lines]
    start = y - (len(lines)-1)*6.7
    add(f'<text x="{x}" y="{start}" text-anchor="middle" font-family="Arial, sans-serif" font-size="{size}" font-weight="{weight}" fill="#172434">')
    for i,t in enumerate(lines): add(f'<tspan x="{x}" dy="{0 if i==0 else 13.4}">{escape(t)}</tspan>')
    add('</text>')
def oval(x,y,rx,lines):
    add(f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="22" fill="#f8fafc" stroke="#34465b" stroke-width="1.3"/>')
    label(x,y+4,lines)
def actor(x,y,name):
    add(f'<circle cx="{x}" cy="{y-25}" r="8" fill="white" stroke="#263b52" stroke-width="1.35"/>')
    line([(x,y-17),(x,y+12)]); line([(x-12,y-8),(x+12,y-8)])
    line([(x,y+12),(x-11,y+29)]); line([(x,y+12),(x+11,y+29)])
    add(f'<rect x="{x-42}" y="{y+32}" width="84" height="20" fill="white"/>')
    label(x,y+47,name,11.7,'bold')

# Fronteira e atores. As linhas de associação não indicam fluxo temporal.
add('<rect x="78" y="18" width="449" height="754" fill="white" stroke="#405369" stroke-width="1.4"/>')
label(302,40,'SQUAD Estoque',12.8,'bold')

# Casos compartilhados e associações a ambos os atores.
for y,txt in [(80,'Realizar login'),(137,'Encerrar sessão')]:
    line([(48,395),(67,395),(67,y),(214,y)])
    line([(557,395),(537,395),(537,y),(391,y)])
    oval(302.5,y,88,txt)

vendor = [
    (238,['Buscar produto e','consultar grade']),
    (301,['Selecionar SKU e','retomar pendência']),
    (364,'Registrar Vendeu'),
    (427,'Registrar Não tinha'),
    (490,'Registrar Desistiu'),
]
lojista = [
    (206,['Cadastrar/editar produto','e grade']),
    (262,'Gerenciar foto do modelo'),
    (318,'Registrar entrada'),
    (374,'Registrar saída manual'),
    (430,'Ajustar saldo com motivo'),
    (486,'Importar/confirmar NF-e'),
    (542,'Consultar saldos zerados'),
    (598,'Consultar rupturas'),
    (654,'Consultar movimentações'),
    (710,'Consultar atendimentos'),
]
for y,txt in vendor:
    line([(48,395),(88,y),(111,y)])
    oval(199,y,88,txt)
for y,txt in lojista:
    line([(557,395),(518,y),(500,y)])
    oval(406,y,94,txt)
actor(35,395,'VENDEDOR'); actor(570,395,'LOJISTA')

svg = f'<svg xmlns="http://www.w3.org/2000/svg" width="160mm" height="209mm" viewBox="0 0 {W} {H}" role="img" aria-label="Diagrama UML de casos de uso do SQUAD Estoque com os atores Vendedor e Lojista"><rect width="{W}" height="{H}" fill="white"/>' + ''.join(shapes) + '</svg>'
Path(__file__).with_suffix('.svg').write_text(svg, encoding='utf-8')
