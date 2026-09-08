import { model, save } from './data.js';
import { eventScreens } from './client-event.js';
import { esc, link, title } from './ui.js';
const app=document.querySelector('#app');
const nav=[['welcome','Overview'],['schedule','Schedule'],['teams','Teams'],['projects','Projects'],['people','People'],['messages','Messages']];
export function render() {
  const q=new URLSearchParams(location.search), screen=q.get('screen')||'access', state=q.get('state')||'default';
  const content=eventScreens(screen,state)||title('DESIGN ARTIFACT','Screen not found','Return to the event entrance.',link('access','Event entrance'));
  document.title=`${screen[0].toUpperCase()+screen.slice(1)} · FaithTech Toronto`;
  app.innerHTML=`<header class="topbar"><a class="brand" href="?screen=welcome" data-nav>faithtech<span class="brand-mark">↗</span></a><span class="divider"></span><span class="header-context">TORONTO / BUILD NIGHT</span><span class="push"></span><span class="event-date">09 SEP 2026</span>${link('profile','AM','ghost')}</header>${screen==='access'?'':`<nav class="main-nav" aria-label="Event navigation">${nav.map(([id,label])=>`<a href="?screen=${id}" data-nav ${screen===id?'aria-current="page"':''}>${label}</a>`).join('')}</nav>`}<main id="main" class="page" tabindex="-1">${content}</main><footer class="site-footer"><span>Made for community. Built with purpose.</span><span>FaithTech Toronto · 2026</span></footer>`;
}
export function navigate(screen,params={}) { history.pushState({},'', '?'+new URLSearchParams({screen,...params})); render(); document.querySelector('#main').focus({preventScroll:true}); window.scrollTo(0,0); }
document.addEventListener('click',e=>{ const a=e.target.closest('[data-nav]'); if(a){e.preventDefault();const q=new URL(a.href).searchParams;navigate(q.get('screen'),Object.fromEntries([...q].filter(([k])=>k!=='screen')));} });
document.addEventListener('submit',e=>{const f=e.target.closest('[data-form]');if(!f)return;e.preventDefault();const d=Object.fromEntries(new FormData(f));if(f.dataset.form==='access'){const p=model.participants.find(p=>p.email.toLowerCase()===d.email.toLowerCase()&&p.code===d.code);if(!p){f.querySelector('.form-error').textContent='We couldn’t match those details. Check your email and entry code.';return;}model.signedIn=true;save();navigate('countdown');}});
window.addEventListener('popstate',render);
render();
