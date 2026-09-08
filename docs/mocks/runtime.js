import { model, save, reset } from './data.js';
import { eventScreens } from './client-event.js';
import { projectScreens } from './client-projects.js';
import { socialScreens } from './client-social.js';
import { activityScreens } from './client-activities.js';
import { adminEventScreens } from './admin-event.js';
import { adminActivityScreens } from './admin-activities.js';
import { submitParticipant } from './interactions.js';
import { submitAdmin } from './admin-interactions.js';
import { submitAdminActivity } from './admin-activity-interactions.js';
import { draw, particles, stopEffects, toggleSound } from './raffle.js';
import { title, link } from './ui.js';
import { catalog } from './catalog.js';
import { shell } from './shell.js';
import { applyState } from './states.js';
import { updateClock } from './clock.js';
import { openDialog, closeDialog, cancelDialog, requestLeave, discard, clean, submittingForm } from './overlay.js';
const app=document.querySelector('#app');
const query=()=>new URLSearchParams(location.search);
function render(){
  stopEffects();document.querySelector('#toast').textContent='';
  const q=query(),screen=q.get('screen')||'catalog',state=q.get('state')||'default',item=q.get('item');
  if(q.has('scenario')){model.event.liturgy=q.get('scenario')==='future';model.event.name=model.event.liturgy?'FaithTech Toronto · October Build Night':'FaithTech Toronto AI Build Event';model.event.date=model.event.liturgy?'2026-10-14':'2026-09-09';save();}
  const content=(screen==='catalog'?catalog():null)||adminEventScreens(screen,state)||adminActivityScreens(screen,state)||eventScreens(screen,state)||projectScreens(screen,state,item)||socialScreens(screen,state,item,q.get('search')||'')||activityScreens(screen,state,item)||title('DESIGN ARTIFACT','This page has wandered off.','Return to the screen index to find your place.',link('catalog','All screens','primary'));
  document.title=`${screen.replaceAll('-',' ')} · FaithTech Toronto`;
  app.innerHTML=shell(applyState(content,screen,state),screen,state);
  if(q.has('dialog'))openDialog(q.get('dialog'),item);
  if(state==='saved')document.querySelector('#toast').textContent='Your changes are saved for this session.';
  if(state==='winner'||state==='drawing')particles();
  updateClock(navigate,false);
}
function navigate(screen,params={}){requestLeave(()=>{clean();closeDialog();history.pushState({},'','?'+new URLSearchParams({screen,...params}));render();document.querySelector('#main').focus({preventScroll:true});window.scrollTo(0,0);});}
document.addEventListener('click',e=>{
  const anchor=e.target.closest('[data-nav]');if(anchor&&!e.ctrlKey&&!e.metaKey){e.preventDefault();const q=new URL(anchor.href).searchParams;navigate(q.get('screen'),Object.fromEntries([...q].filter(([k])=>k!=='screen')));return;}
  const b=e.target.closest('[data-action]');if(!b)return;const a=b.dataset.action;
  if(a==='dialog'){const q=query();q.set('dialog',b.dataset.dialog);if(b.dataset.item)q.set('item',b.dataset.item);history.pushState({},'','?'+q);openDialog(b.dataset.dialog,b.dataset.item);}
  if(a==='close-dialog')cancelDialog();
  if(a==='start-quiz'){model.quizIndex=0;model.answers={};save();navigate('quiz');}
  if(a==='next-question'){if(model.quizIndex<model.questions.length-1){model.quizIndex++;save();navigate('quiz');}else navigate('quiz',{state:'results'});}
  if(a==='next-draw')navigate('admin-raffle');
  if(a==='sound')toggleSound(b);
  if(a==='retry-message'||a==='reconnect')navigate(query().get('screen'),{item:query().get('item')||'sarah'});
  if(a==='reset')requestLeave(()=>{clean();reset();navigate('catalog');});
  if(a==='pause-clock'){model.followClock=false;save();b.textContent='Paused';}
});
document.addEventListener('submit',async e=>{
  const f=e.target.closest('[data-form]');if(!f)return;e.preventDefault();const d=Object.fromEntries(new FormData(f)),id=f.dataset.dialog||f.dataset.form;
  if(id==='unsaved'){discard();return;}
  if(id==='review'){navigate(d.screen,{state:d.state,scenario:d.scenario});return;}
  if(id==='clock'){model.clock=d.clock;model.clockSpeed=Number(d.speed);model.followClock=true;model.lastScheduledScreen='';save();navigate('countdown');return;}
  if(id==='draw'){draw(navigate);return;}
  if(query().get('state')==='save-error'&&!f.dataset.retried){f.dataset.retried='true';f.querySelector('.form-error').textContent='We couldn’t save this change. Your edits are kept here. Please try again.';return;}
  submittingForm(true);
  try{const result=submitParticipant(id,d,f.dataset.item,navigate)||await submitAdmin(id,d,f.dataset.item,navigate)||submitAdminActivity(id,d,f.dataset.item,navigate);if(typeof result==='string')f.querySelector('.form-error').textContent=result;}
  catch{f.querySelector('.form-error').textContent='We couldn’t save this change. Try a smaller logo or reset the sample session.';}
  finally{submittingForm(false);}
});
window.addEventListener('popstate',()=>{clean();closeDialog();render();});
setInterval(()=>updateClock(navigate),1000);
render();
