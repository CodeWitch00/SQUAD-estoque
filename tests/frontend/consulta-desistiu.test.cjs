const {test,before,after}=require('node:test');
const assert=require('node:assert/strict');
const path=require('node:path');
const {chromium}=require('playwright');
let browser;
before(async()=>{browser=await chromium.launch({headless:true});});
after(async()=>{await browser?.close();});
async function open(t,width=390){
 const page=await browser.newPage({viewport:{width,height:844}});t.after(()=>page.close());
 await page.route('http://consulta.test/**',route=>route.fulfill({contentType:'text/html',body:`<div class="consulta-vendedor"><div id="consulta-antiforgery"><input name="__RequestVerificationToken" value="token"></div><section class="consulta-grade"><h1>Runner</h1><div class="consulta-grade-item"><input type="radio" name="skuSelecionado" value="sku" data-numeracao="39" data-saldo="3"></div></section><section class="consulta-atendimento-acoes"><p id="consulta-atendimento-resumo"></p><button class="consulta-acao" data-resultado="desistiu" data-desistiu-url="/Estoque/RegistrarDesistiu">Desistiu</button><p id="consulta-venda-retorno" hidden></p><p id="consulta-acao-feedback" tabindex="-1" role="status" hidden></p><a class="consulta-nova-consulta--conclusao" href="/Estoque/Consulta" hidden>Nova consulta</a></section></div>`}));
 await page.goto('http://consulta.test/Estoque/Consulta?termo=Runner&produtoId=produto#grade');
 await page.addStyleTag({path:path.resolve(__dirname,'../../src/SquadEstoque.Web/wwwroot/css/site.css')});
 await page.evaluate(()=>{
 window.requests=[];
 window.fetch=async(url,options)=>{
 window.requests.push({url,method:options.method,fields:options.body?[...options.body.entries()]:[]});
 if(options.method==='POST')return await new Promise(resolve=>window.respond=data=>resolve({ok:true,headers:new Headers({'content-type':'application/json'}),json:async()=>data}));
 if(window.failGet)throw Error('offline');
 return {ok:true,text:async()=>'<div class="consulta-vendedor"><h1>Qual modelo o cliente procura?</h1><form action="/Estoque/Consulta"><label for="Termo">Produto</label><input id="Termo" name="Termo" value=""><button>Buscar</button></form></div>'};
 };
 });
 await page.addScriptTag({path:path.resolve(__dirname,'../../src/SquadEstoque.Web/wwwroot/js/consulta.js')});return page;
}
for(const width of [390,1280])for(const selected of [false,true])test(`Desistiu ${width}px, SKU ${selected}: retorno, foco, saldo e um POST`,async t=>{
 const page=await open(t,width);if(selected)await page.locator('[name=skuSelecionado]').check();
 const button=page.getByRole('button',{name:'Desistiu'});assert.equal(await button.isVisible(),true);assert.ok((await button.boundingBox()).height>=44);
 await button.focus();await page.keyboard.press('Enter');await button.dispatchEvent('click');
 assert.equal(await page.evaluate(()=>window.requests.length),1);
 assert.equal(await page.locator('[data-saldo]').getAttribute('data-saldo'),'3');
 await page.evaluate(()=>window.respond({resultado:'desistiu',atendimentoEncerrado:true,novaConsultaUrl:'/Estoque/Consulta'}));
 await page.waitForFunction(()=>document.activeElement?.name==='Termo');
 assert.equal(await page.locator('[name=Termo]').inputValue(),'');assert.equal(await page.locator('.consulta-grade').count(),0);
 assert.match(await page.locator('#consulta-desistiu-retorno').innerText(),/Sem venda, sem ruptura e sem alteração/);
 assert.equal(new URL(page.url()).search,'');assert.equal(new URL(page.url()).hash,'');
 assert.deepEqual(await page.evaluate(()=>window.requests[0].fields),[['__RequestVerificationToken','token']]);
 await page.keyboard.press('Tab');assert.equal(await page.evaluate(()=>document.activeElement.textContent),'Buscar');
});
test('resposta inválida preserva contexto e permite repetir sem sucesso fictício',async t=>{
 const page=await open(t);await page.locator('[name=skuSelecionado]').check();await page.getByRole('button',{name:'Desistiu'}).click();
 await page.evaluate(()=>window.respond({resultado:'outro'}));await page.waitForFunction(()=>!document.querySelector('#consulta-acao-feedback').hidden);
 assert.match(await page.locator('#consulta-acao-feedback').innerText(),/Não foi possível/);assert.equal(await page.locator('[name=skuSelecionado]').isChecked(),true);assert.equal(await page.getByRole('button',{name:'Desistiu'}).isEnabled(),true);
});
test('falha na nova busca após confirmação mantém link e não repete POST',async t=>{
 const page=await open(t);await page.evaluate(()=>window.failGet=true);await page.getByRole('button',{name:'Desistiu'}).click();await page.evaluate(()=>window.respond({resultado:'desistiu',atendimentoEncerrado:true,novaConsultaUrl:'/Estoque/Consulta'}));
 await page.waitForFunction(()=>document.activeElement?.textContent==='Nova consulta');assert.equal(await page.getByRole('button',{name:'Desistiu'}).isDisabled(),true);assert.equal(await page.evaluate(()=>window.requests.filter(r=>r.method==='POST').length),1);
});
