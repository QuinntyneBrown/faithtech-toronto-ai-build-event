import { model } from './data.js';
import { esc, field, area, select, check } from './ui.js';
import { link } from './ui.js';
import { adminNav } from './admin-event.js';
export function adminDialogContent(id,item) {
  if(id==='admin-navigation')return ['Host navigation',`<div class="cs-stack">${adminNav.map(([s,l])=>link(s,l)).join('')}</div>`,null];
  const p=model.participants.find(x=>x.id===item)||{},s=model.stages.find(x=>x.id===item)||{};
  if(id==='participant-edit')return [item?'Edit participant':'Make room for someone.',field('name','Full name',p.name)+field('email','Email address',p.email,'email')+field('code','Entry code',p.code||'BUILD26')+field('role','Role or skills',p.role,'text','',false),'Save participant'];
  if(id==='participant-remove')return ['Remove '+(p.name||'participant')+'?',`<p>${esc(p.name)} will lose access to this event and be removed from team rosters.</p>`,'Remove participant'];
  if(id==='stage-edit')return [item?'Edit stage':'Add a moment.',field('name','Stage name',s.name)+`<div class="split">${field('start','Start time',s.start||'18:00','time')}${field('end','End time',s.end||'18:15','time')}</div>`+select('screen','Participant screen',[['welcome','Welcome'],['teams','Team selection'],['projects','Project selection'],['build','Building guidance'],['people','Networking'],['quiz','Quiz'],['raffle','Raffle'],['demos','Demos'],['recap','Closing & recap']],s.screen||'welcome')+area('content','Stage instructions',s.content)+field('link','Resource URL',s.link,'url','Optional link shown with the stage instructions.',false),'Save stage'];
  if(id==='stage-remove')return ['Remove this stage?',`<p>“${esc(s.name)}” will be removed from the event schedule.</p>`,'Remove stage'];
  if(id==='stage-reorder')return ['Move this stage',select('position','New position',model.stages.map((x,i)=>[String(i),`${i+1} · ${x.name}`]),String(model.stages.findIndex(x=>x.id===item)))+'<p class="muted">Stages will retain their durations and receive consecutive times from the event start.</p>','Move stage'];
  return null;
}
