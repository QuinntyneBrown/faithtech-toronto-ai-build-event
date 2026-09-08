import { model, save } from './data.js';
import { eventScreens } from './client-event.js';
import { projectScreens } from './client-projects.js';
import { dialogContent, dialogMarkup } from './dialogs.js';
import { submitParticipant } from './interactions.js';
import { socialScreens } from './client-social.js';
import { esc, link, title } from './ui.js';
const app=document.querySelector('#app');
const nav=[['welcome','Overview'],['schedule','Schedule'],['teams','Teams'],['projects','Projects'],['people','People'],['messages','Messages']];
export function render() {
  const q=new URLSearchParams(location.search), screen=q.get('screen')||'access', state=q.get('state')||'default';
  const content=eventScreens(screen,state)||projectScreens(screen,state,q.get('item'))||socialScreens(screen,state,q.get('item'),q.get('search')||'')||title('DESIGN ARTIFACT','Screen not found','Return to the event entrance.',link('access','Event entrance'));
  document.title=`${screen[0].toUpperCase()+screen.slice(1)} · FaithTech Toronto`;
  app.innerHTML=`<header class="topbar"><a class="brand" href="?screen=welcome" data-nav>faithtech<span class="brand-mark">↗</span></a><span class="divider"></span><span class="header-context">TORONTO / BUILD NIGHT</span><span class="push"></span><span class="event-date">09 SEP 2026</span>${link('profile','AM','ghost')}</header>${screen==='access'?'':`<nav class="main-nav" aria-label="Event navigation">${nav.map(([id,label])=>`<a href="?screen=${id}" data-nav ${screen===id?'aria-current="page"':''}>${label}</a>`).join('')}</nav>`}<main id="main" class="page" tabindex="-1">${content}</main><footer class="site-footer"><span>Made for community. Built with purpose.</span><span>FaithTech Toronto · 2026</span></footer>`;
  if(q.has('dialog'))openDialog(q.get('dialog'),q.get('item'));
  if(state==='saved')document.querySelector('#toast').textContent='Your changes are saved for this session.';
}
function openDialog(id,item) {const overlay=document.querySelector('#overlay');overlay.innerHTML=dialogMarkup(id,item,dialogContent(id,item));if(overlay.innerHTML&&!overlay.open)overlay.showModal();}
function closeDialog(){const overlay=document.querySelector('#overlay');overlay.close();const q=new URLSearchParams(location.search);q.delete('dialog');history.replaceState({},'','?'+q);}
export function navigate(screen,params={}) { closeDialog();history.pushState({},'', '?'+new URLSearchParams({screen,...params})); render(); document.querySelector('#main').focus({preventScroll:true}); window.scrollTo(0,0); }
document.addEventListener('click',e=>{ const a=e.target.closest('[data-nav]'); if(a){e.preventDefault();const q=new URL(a.href).searchParams;navigate(q.get('screen'),Object.fromEntries([...q].filter(([k])=>k!=='screen')));} });
document.addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(!b)return;if(b.dataset.action==='dialog'){const q=new URLSearchParams(location.search);q.set('dialog',b.dataset.dialog);if(b.dataset.item)q.set('item',b.dataset.item);history.pushState({},'','?'+q);openDialog(b.dataset.dialog,b.dataset.item);}if(b.dataset.action==='close-dialog')closeDialog();});
document.addEventListener('submit',e=>{const f=e.target.closest('[data-form]');if(!f)return;e.preventDefault();const d=Object.fromEntries(new FormData(f));const result=submitParticipant(f.dataset.dialog||f.dataset.form,d,f.dataset.item,navigate);if(typeof result==='string')f.querySelector('.form-error').textContent=result;});
document.querySelector('#overlay').addEventListener('cancel',e=>{e.preventDefault();closeDialog();});
window.addEventListener('popstate',render);
render();
