import { model } from './data.js';
import { title, link, card, pill, esc, select, field, button, form, url } from './ui.js';
export const screens=[
  ['access','Participant access','Participant',['default','rejected','expired','loading']],
  ['countdown','Countdown & venue','Participant',['default','offline']],
  ['welcome','Welcome','Participant',['default','transition']],
  ['schedule','Event schedule','Participant',['default']],
  ['teams','Team selection','Participant',['default','random','empty','full']],
  ['team','Team detail','Participant',['default']],
  ['projects','Project selection','Participant',['default','restricted','empty']],
  ['project','Project detail & links','Participant',['default']],
  ['build','Building guidance','Participant',['default']],
  ['profile','Your profile','Participant',['default','saved','save-error']],
  ['people','People directory','Community',['default','empty','loading']],
  ['person','Participant profile','Community',['default']],
  ['connections','Recommended connections','Community',['default','empty']],
  ['messages','Conversations','Community',['default','empty','sending','failed','offline']],
  ['quiz','Community quiz','Activities',['intro','waiting','default','selected','feedback','results','closed']],
  ['raffle','Raffle & celebration','Activities',['default','drawing','winner','exhausted']],
  ['demos','Demo schedule','Activities',['default','empty']],
  ['demo','On-stage presentation','Activities',['default']],
  ['showcase','Project showcase','Activities',['default']],
  ['recap','Closing & recap','Activities',['default']],
  ['admin-login','Administrator sign-in','Administration',['default','rejected']],
  ['admin-events','Event list','Administration',['default']],
  ['admin-overview','Event overview','Administration',['default','denied','loading']],
  ['admin-settings','Event & venue configuration','Administration',['default','new','saved','save-error']],
  ['admin-participants','Participant management','Administration',['default','empty']],
  ['admin-schedule','Schedule & stage content','Administration',['default','timing-error']],
  ['admin-teams','Team management','Administration',['default']],
  ['admin-projects','Project management','Administration',['default']],
  ['admin-quizzes','Quiz editor','Administration',['default']],
  ['admin-quiz-results','Quiz responses','Administration',['default']],
  ['admin-raffle','Raffle draw console','Administration',['default','drawing','winner','exhausted']],
  ['admin-demos','Demo lineup editor','Administration',['default']],
  ['not-found','Unavailable screen','Shared states',['default']]
];
export const dialogs=[
  ['join-team','Join team','teams','north'],['leave-team','Change team','team','north'],['assign','Random team assignment','teams',''],
  ['select-project','Choose project','project','care'],['propose-project','Propose project','projects',''],['project-links','Repository & demo links','project','care'],
  ['new-message','New conversation','messages',''],['sign-out','Sign out','welcome',''],['account','Account menu','welcome',''],['navigation','Mobile navigation','welcome',''],['unsaved','Unsaved changes','profile',''],
  ['participant-edit','Add participant','admin-participants',''],['participant-edit','Edit participant','admin-participants','sarah'],['participant-remove','Remove participant','admin-participants','sarah'],
  ['stage-edit','Add stage','admin-schedule',''],['stage-edit','Edit stage & content','admin-schedule','develop'],['stage-reorder','Reorder stage','admin-schedule','develop'],['stage-remove','Remove stage','admin-schedule','develop'],
  ['team-edit','Add team','admin-teams',''],['team-edit','Edit team & roster','admin-teams','north'],['team-remove','Remove team','admin-teams','north'],['assign-all','Assignment preview','admin-teams',''],
  ['project-edit','Add project','admin-projects',''],['project-edit','Edit project','admin-projects','care'],['project-remove','Remove project','admin-projects','care'],
  ['question-edit','Add question','admin-quizzes',''],['question-edit','Edit answers','admin-quizzes','q1'],['question-remove','Remove question','admin-quizzes','q1'],
  ['prize-edit','Add prize','admin-raffle',''],['prize-edit','Edit prize','admin-raffle','book'],['prize-remove','Remove prize','admin-raffle','book'],['draw','Confirm raffle draw','admin-raffle',''],
  ['demo-edit','Add demo','admin-demos',''],['demo-edit','Edit demo','admin-demos','care'],['demo-reorder','Reorder demos','admin-demos','care'],['demo-remove','Remove demo','admin-demos','care'],['admin-navigation','Mobile host navigation','admin-overview','']
];
export function catalog(){return title('FAITHTECH TORONTO / DESIGN ARTIFACTS','An evening, fully imagined.','Explore every screen, dialog, and state. A clickable, local prototype built on the Cornerstone light theme.',`<a class="cs-button cs-button--secondary" href="../../design-system/">Design system ↗</a>`)+`<div class="feature-panel"><p class="eyebrow">SEPTEMBER 09 / BUILD TOGETHER</p><h2>People first.<br>Possibility follows.</h2><p>Start with a complete experience, or jump straight to a detail below.</p><div class="cs-cluster">${link('access','Participant experience','primary',{scenario:'september'})}${link('admin-login','Host experience')}${link('project','Future event · Liturgy enabled','secondary',{item:'care',scenario:'future'})}</div><p class="small muted section">Sample people, venue, links, prizes, and schedule. No real registration, messages, or uploads.</p></div>${[...new Set(screens.map(s=>s[2]))].map(group=>`<section class="catalog-group"><p class="eyebrow">${group.toUpperCase()}</p><div class="grid-three">${screens.filter(s=>s[2]===group).map(([id,label,,states])=>card(`<h2>${esc(label)}</h2><div class="cs-cluster">${states.map(state=>link(id,state==='default'?'Open screen':state.replaceAll('-',' '),'ghost',{state})).join('')}</div>`)).join('')}</div></section>`).join('')}<section class="section"><h2>Dialogs & overlays</h2><div class="grid-three">${dialogs.map(([id,label,screen,item])=>`<a class="cs-card catalog-item" data-nav href="${url(screen,{dialog:id,item})}"><span class="eyebrow">${screen.startsWith('admin')?'HOST':'PARTICIPANT'}</span><h3>${label}</h3><span class="muted small">Open in context ↗</span></a>`).join('')}</div></section>`;}
export function reviewer(screen,state){return `<details class="reviewer"><summary>Design review controls · ${esc(screen)} / ${esc(state)}</summary><div class="reviewer-body"><div class="cs-cluster">${link('catalog','All screens & dialogs','secondary')}<a href="../../design-system/" class="cs-button cs-button--ghost">Design system ↗</a>${button('reset','Reset sample session','ghost')}</div><hr>${form('review',`<div class="grid-three">${select('screen','Screen',screens.map(([id,l])=>[id,l]),screen)}${select('state','State',['default','loading','empty','offline','reconnecting','saved','save-error','intro','waiting','selected','feedback','results','closed','drawing','winner','exhausted','denied','rejected','expired','random','restricted','full','new','transition','timing-error','sending','failed'],state)}${select('scenario','Event scenario',[['september','September 9 · Liturgy off'],['future','Future event · Liturgy on']],model.event.liturgy?'future':'september')}</div>`,'Show artifact')}<hr>${form('clock',`<div class="grid-three">${field('clock','Simulated time',model.clock.slice(0,5),'time')}${select('speed','Clock speed',[['1','Real time'],['60','1 minute per second']],String(model.clockSpeed||60))}<div><p class="cs-label">Scheduled flow</p><p class="muted">${model.followClock?'Playing':'Paused'} · <span id="review-clock">${esc(model.clock)}</span></p>${button('pause-clock','Pause','secondary')}</div></div>`,'Play scheduled flow')}<p class="small muted">All edits stay in this browser session. The raffle uses a repeatable sample winner sequence. Use Reset to restore the complete September fixture.</p></div></details>`;}
