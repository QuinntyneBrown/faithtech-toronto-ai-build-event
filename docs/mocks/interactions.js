import { model, save, currentId } from './data.js';
export function submitParticipant(id,data,item,navigate) {
  if(id==='quiz'){model.answers[model.questions[model.quizIndex].id]=data.answer;save();navigate('quiz',{state:'feedback'});return true;}
  if(id==='profile'){Object.assign(model.profile,data);const p=model.participants.find(p=>p.id===currentId());if(p)Object.assign(p,data);save();navigate('profile',{state:'saved'});return true;}
  if(id==='search'){navigate(new URLSearchParams(location.search).get('screen')||'people',{search:data.search});return true;}
  if(id==='new-message'){navigate('messages',{item:data.to,state:'empty'});return true;}
  if(id==='message'){if(!data.text.trim())return 'Write a message before sending.';model.messages.push({from:currentId(),to:data.to,text:data.text,time:model.clock});save();navigate('messages',{item:data.to});return true;}
  if(id==='access') {
    const p=model.participants.find(p=>p.email.toLowerCase()===data.email.toLowerCase()&&p.code===data.code);
    if(!p)return 'We couldn’t match those details. Check your email and entry code.';
    model.signedIn=true; model.profile={...model.profile,...p}; save(); navigate('countdown'); return true;
  }
  if(id==='join-team'||id==='assign') {
    const team=id==='assign'?model.teams.find(t=>t.members.length<t.capacity):model.teams.find(t=>t.id===item);
    if(!team||team.members.length>=team.capacity)return 'This team has no available seats. Choose another team or ask your host.';
    model.teams.forEach(t=>t.members=t.members.filter(id=>id!==currentId()));team.members.push(currentId());model.team=team.id;save();navigate('team',{item:team.id});return true;
  }
  if(id==='leave-team'){model.teams.forEach(t=>t.members=t.members.filter(id=>id!==currentId()));model.team='';save();navigate('teams');return true;}
  if(id==='select-project'){model.project=item;save();navigate('project',{item});return true;}
  if(id==='propose-project') {
    if(!model.event.proposals||new URLSearchParams(location.search).get('state')==='restricted')return 'Your host has restricted project choices for this event.';
    const p={...data,id:'proposal-'+Date.now(),category:'PARTICIPANT IDEA',repository:'',demo:'',liturgy:'',team:model.teams.find(t=>t.id===model.team)?.name||'Open to a team'};model.projects.push(p);save();navigate('project',{item:p.id});return true;
  }
  if(id==='project-links'){const p=model.projects.find(p=>p.id===item);if(!p)return 'This project is no longer available.';Object.assign(p,data);save();navigate('project',{item});return true;}
  if(id==='sign-out'){model.signedIn=false;model.admin=false;save();navigate('access');return true;}
  return false;
}
