export class TerminalPage {
  constructor(page) { this.page = page; }
  async open(title, subtitle) {
    await this.page.setContent('<html lang="en"><head><style>body{margin:0;background:#fffdf4;color:#191b19;font:22px/1.5 system-ui}header{padding:32px 54px;border-bottom:1px solid #d9d9cc}h1{font-size:34px;margin:0}p{margin:8px 0 0;color:#5c645e}main{padding:28px 54px}pre{margin:0;white-space:pre-wrap;overflow-wrap:anywhere;font:21px/1.55 Consolas,monospace}#result{margin-top:24px;padding:22px;background:#f0f2e7;border-radius:12px;color:#17392e}</style></head><body><header><h1></h1><p></p></header><main><pre id="command"></pre><pre id="result"></pre></main></body></html>');
    await this.page.locator('h1').evaluate((el, text) => el.textContent = text, title);
    await this.page.locator('header p').evaluate((el, text) => el.textContent = text, subtitle);
  }
  async command(text) {
    await this.page.locator('#command').evaluate((el, text) => el.textContent = text, text);
    await this.page.locator('#result').evaluate(el => el.textContent = '');
  }
  async result(text) { await this.page.locator('#result').evaluate((el, text) => el.textContent = text, text); }
}
