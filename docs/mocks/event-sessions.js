import { initial,model,save } from './data.js';
function snapshot(){const {archivedEvents,...eventSession}=model;return structuredClone(eventSession);}
export function createEvent(details){
  const archivedEvents=[...(model.archivedEvents||[]),snapshot()],next=structuredClone(initial);
  for(const key of ['participants','teams','projects','stages','questions','prizes','demos','messages','winners'])next[key]=[];
  next.event={...next.event,...details,liturgy:details.liturgy==='on'};next.admin=model.admin;next.archivedEvents=archivedEvents;
  Object.keys(model).forEach(k=>delete model[k]);Object.assign(model,next);save();
}
export function switchEvent(index){
  const entries=[...(model.archivedEvents||[])],selected=entries[index];if(!selected?.event)return;
  entries[index]=snapshot();Object.keys(model).forEach(k=>delete model[k]);Object.assign(model,selected,{archivedEvents:entries});model.followClock=false;save();
}
