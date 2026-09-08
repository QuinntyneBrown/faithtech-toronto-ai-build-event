import { model } from './data.js';
import { esc, field, area, button, link, select } from './ui.js';
export function dialogContent(id,item) {
  const team=model.teams.find(t=>t.id===item)||model.teams[0], project=model.projects.find(p=>p.id===item)||model.projects[0];
  const definitions={
    'draw': ['Ready for a little celebration?',`<p>Draw from ${model.participants.filter(p=>!model.winners.some(w=>w.id===p.id)).length} eligible participants. Previous winners will be excluded.</p><p class="muted">The review prototype uses a repeatable sample draw.</p>`,'Start the draw'],
    'new-message': ['Start a conversation',select('to','Who would you like to meet?',model.participants.filter(p=>p.id!=='alex').map(p=>[p.id,p.name]),'sarah'),'Open conversation'],
    'join-team': ['A place at '+(team?.name||'your table'),`<p>Join ${esc(team?.name)} and bring your skills to the table. ${model.team?'You’ll leave your current team.':''}</p>`,'Join team'],
    'leave-team': ['Find another team?','<p>You’ll leave your current table and can choose a new team.</p>','Leave team'],
    'assign': ['Let’s find your people.','<p>We’ll place you in a team with an available seat.</p>','Assign my team'],
    'select-project': ['Build '+(project?.name||'this project')+'?',`<p>${esc(project?.description)} Your selection will appear in your building guide.</p>`,'Choose project'],
    'propose-project': ['Bring your idea.',field('name','Project name')+area('description','What would you like to build?')+area('problem','Who needs this, and why?')+field('outcome','One useful outcome'),'Share proposal'],
    'project-links': ['Share your work.',field('repository','Repository URL',project?.repository,'url','',false)+field('demo','Demo URL',project?.demo,'url','',false),'Save links'],
    'unsaved': ['Leave without saving?','<p>Your unsaved edits will be discarded. You can stay here to finish them.</p>','Discard changes'],
    'sign-out': ['See you soon.','<p>You can return to the event using your email and entry code.</p>','Sign out'],
    'account': ['Your evening',`<div class="cs-stack">${link('profile','Edit my profile')}${link('team','My team')}${link('access','Event entrance')}${button('dialog','Sign out','ghost','data-dialog="sign-out"')}</div>`,null],
    'navigation': ['Explore the event',`<div class="cs-stack">${[['welcome','Overview'],['schedule','Schedule'],['teams','Teams'],['projects','Projects'],['build','Build'],['people','People'],['messages','Messages'],['quiz','Quiz'],['raffle','Raffle'],['demos','Demos'],['recap','Recap']].map(([s,l])=>link(s,l)).join('')}</div>`,null]
  };
  return definitions[id]||null;
}
export function dialogMarkup(id,item,definition) {
  if(!definition)return '';
  const [heading,body,submit]=definition;
  return `<form data-form="dialog" data-dialog="${esc(id)}" data-item="${esc(item||'')}"><header><p class="eyebrow">FAITHTECH TORONTO</p><h2 id="overlay-title">${esc(heading)}</h2></header><div><div class="form-error" role="alert"></div>${body}</div><footer>${button('close-dialog',submit?'Cancel':'Close','secondary')}${submit?`<button class="cs-button" type="submit">${submit}</button>`:''}</footer></form>`;
}
