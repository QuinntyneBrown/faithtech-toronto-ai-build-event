import { model, save } from './data.js';
import { eventScreens } from './client-event.js';
import { projectScreens } from './client-projects.js';
import { dialogContent, dialogMarkup } from './dialogs.js';
import { submitParticipant } from './interactions.js';
import { socialScreens } from './client-social.js';
import { activityScreens } from './client-activities.js';
import { draw, particles, stopEffects, toggleSound } from './raffle.js';
import { adminEventScreens, adminNav } from './admin-event.js';
import { adminDialogContent } from './admin-dialogs.js';
import { submitAdmin } from './admin-interactions.js';
import { adminActivityScreens } from './admin-activities.js';
import { adminActivityDialogContent } from './admin-activity-dialogs.js';
import { submitAdminActivity } from './admin-activity-interactions.js';
import { esc, link, title } from './ui.js';
const app=document.querySelector('#app');
const nav=[['welcome','Overview'],['schedule','Schedule'],['teams','Teams'],['projects','Projects'],['people','People'],['messages','Messages']];
export function render() {
  stopEffects();document.querySelector('#toast').textContent='';
  const q=new URLSearchParams(location.search), screen=q.get('screen')||'access', state=q.get('state')||'default';
  const content=adminEventScreens(screen,state)||adminActivityScreens(screen,state)||eventScreens(screen,state)||projectScreens(screen,state,q.get('item'))||socialScreens(screen,state,q.get('item'),q.get('search')||'')||activityScreens(screen,state,q.get('item'))||title('DESIGN ARTIFACT','Screen not found','Return to the event entrance.',link('access','Event entrance'));
  document.title=`${screen[0].toUpperCase()+screen.slice(1)} · FaithTech Toronto`;
  const admin=screen.startsWith('admin-')&&screen!=='admin-login';
  const main=`<main id="main" class="page ${admin?'compact':''}" tabindex="-1">${content}</main>`;
  app.innerHTML=`<header class="topbar"><a class="brand" href="?screen=${admin?'admin-events':'welcome'}" data-nav>faithtech<span class="brand-mark">↗</span></a><span class="divider"></span><span class="header-context">${admin?'EVENT HOST / TORONTO':'TORONTO / BUILD NIGHT'}</span><span class="push"></span><span class="event-date">${esc(model.event.date)}</span><button class="cs-button cs-button--ghost mobile-menu" data-action="dialog" data-dialog="${admin?'admin-navigation':'navigation'}">Menu</button><button class="cs-button cs-button--ghost" data-action="dialog" data-dialog="account" aria-label="Your account">${admin?'HOST':'AM'}</button></header>${admin?`<div class="admin-layout"><nav class="admin-nav" aria-label="Admin navigation"><p class="eyebrow">YOUR EVENT</p>${adminNav.map(([id,label])=>`<a href="?screen=${id}" data-nav ${screen===id?'aria-current="page"':''}>${label}</a>`).join('')}<hr>${link('admin-events','All events','ghost')}${link('welcome','Participant preview','ghost')}</nav>${main}</div>`:`${['access','admin-login','catalog'].includes(screen)?'':`<nav class="main-nav" aria-label="Event navigation">${nav.map(([id,label])=>`<a href="?screen=${id}" data-nav ${screen===id?'aria-current="page"':''}>${label}</a>`).join('')}</nav>`}${main}`}<footer class="site-footer"><span>Made for community. Built with purpose.</span><span>FaithTech Toronto · 2026</span></footer>`;
  if(q.has('dialog'))openDialog(q.get('dialog'),q.get('item'));
  if(state==='saved')document.querySelector('#toast').textContent='Your changes are saved for this session.';
  if(state==='winner'||state==='drawing')particles();
}
function openDialog(id,item) {const overlay=document.querySelector('#overlay');overlay.innerHTML=dialogMarkup(id,item,dialogContent(id,item)||adminDialogContent(id,item)||adminActivityDialogContent(id,item));if(overlay.innerHTML&&!overlay.open)overlay.showModal();}
function closeDialog(){const overlay=document.querySelector('#overlay');overlay.close();const q=new URLSearchParams(location.search);q.delete('dialog');history.replaceState({},'','?'+q);}
export function navigate(screen,params={}) { closeDialog();history.pushState({},'', '?'+new URLSearchParams({screen,...params})); render(); document.querySelector('#main').focus({preventScroll:true}); window.scrollTo(0,0); }
document.addEventListener('click',e=>{ const a=e.target.closest('[data-nav]'); if(a){e.preventDefault();const q=new URL(a.href).searchParams;navigate(q.get('screen'),Object.fromEntries([...q].filter(([k])=>k!=='screen')));} });
document.addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(!b)return;if(b.dataset.action==='dialog'){const q=new URLSearchParams(location.search);q.set('dialog',b.dataset.dialog);if(b.dataset.item)q.set('item',b.dataset.item);history.pushState({},'','?'+q);openDialog(b.dataset.dialog,b.dataset.item);}if(b.dataset.action==='close-dialog')closeDialog();});
document.addEventListener('click',e=>{const b=e.target.closest('[data-action]');if(!b)return;const a=b.dataset.action;if(a==='start-quiz'){model.quizIndex=0;model.answers={};save();navigate('quiz');}if(a==='next-question'){if(model.quizIndex<model.questions.length-1){model.quizIndex++;save();navigate('quiz');}else navigate('quiz',{state:'results'});}if(a==='next-draw')navigate('admin-raffle');if(a==='sound')toggleSound(b);if(a==='retry-message')navigate('messages',{item:new URLSearchParams(location.search).get('item')||'sarah'});});
document.addEventListener('submit',async e=>{const f=e.target.closest('[data-form]');if(!f)return;e.preventDefault();const d=Object.fromEntries(new FormData(f));if(f.dataset.dialog==='draw'){draw(navigate);return;}try{const id=f.dataset.dialog||f.dataset.form;const result=submitParticipant(id,d,f.dataset.item,navigate)||await submitAdmin(id,d,f.dataset.item,navigate)||submitAdminActivity(id,d,f.dataset.item,navigate);if(typeof result==='string')f.querySelector('.form-error').textContent=result;}catch{f.querySelector('.form-error').textContent='We couldn’t save this change. Try a smaller logo or reset the sample session.';}});
document.querySelector('#overlay').addEventListener('cancel',e=>{e.preventDefault();closeDialog();});
window.addEventListener('popstate',render);
render();
