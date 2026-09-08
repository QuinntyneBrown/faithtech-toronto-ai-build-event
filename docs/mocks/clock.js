import { model, save } from './data.js';
export function updateClock(navigate,advance=true){
  let [h,m,s=0]=model.clock.split(':').map(Number),seconds=h*3600+m*60+s;
  if(model.followClock&&advance){seconds+=Number(model.clockSpeed||60);model.clock=[Math.floor(seconds/3600)%24,Math.floor(seconds/60)%60,seconds%60].map(n=>String(n).padStart(2,'0')).join(':');save();}
  const now=model.clock.slice(0,5),target=model.stages.find(s=>now>=s.start&&now<s.end)?.screen||(now>=model.event.end?'recap':now<model.event.start?'countdown':'schedule');
  const counter=document.querySelector('#countdown-value'),clock=document.querySelector('#review-clock');
  if(clock)clock.textContent=model.clock;
  if(counter){const [startH,startM]=model.event.start.split(':').map(Number),remaining=Math.max(0,startH*3600+startM*60-seconds);counter.textContent=[Math.floor(remaining/3600),Math.floor(remaining/60)%60,remaining%60].map(n=>String(n).padStart(2,'0')).join(' : ');}
  const screen=new URLSearchParams(location.search).get('screen');
  if(model.followClock&&!screen?.startsWith('admin')&&screen!=='catalog'&&screen!=='access'&&target!==model.lastScheduledScreen){model.lastScheduledScreen=target;save();navigate(target,{state:'transition'});}
}
