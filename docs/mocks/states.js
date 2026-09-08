import { alert, button, title, link } from './ui.js';
export function applyState(content,screen,state){
  if(state==='loading')return title('GETTING THINGS READY','One moment.','We’re preparing this part of your evening.')+`<div class="cs-stack" role="status" aria-busy="true" aria-label="Loading event content"><span class="cs-skeleton"></span><span class="cs-skeleton"></span><span class="cs-skeleton"></span></div><div class="section">${link(screen,'Show loaded screen')}</div>`;
  if(state==='offline')return alert(`You’re offline. Your last view is still here. ${button('reconnect','Reconnect','ghost')}`,'warning')+content;
  if(state==='reconnecting')return alert(`Reconnecting to your event… ${button('reconnect','Try again','ghost')}`)+content;
  if(state==='transition')return alert('A new part of the evening has started. Welcome to the next stage.')+content;
  if(state==='save-error')return alert('Your last change couldn’t be saved. Your edits are still here; try saving again.','error')+content;
  return content;
}
