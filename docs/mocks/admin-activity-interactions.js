import { model, save, currentId } from './data.js';
import { assignmentPreview } from './admin-activity-dialogs.js';
export function submitAdminActivity(id,data,item,navigate){
  if(id==='team-settings'){model.event.teamMode=data.teamMode;save();navigate('admin-teams',{state:'saved'});return true;}
  if(id==='project-settings'){model.event.proposals=data.proposals==='on';save();navigate('admin-projects',{state:'saved'});return true;}
  if(id==='team-edit'){
    const members=Object.keys(data).filter(k=>k.startsWith('member-')).map(k=>k.slice(7)),capacity=Number(data.capacity);
    if(!Number.isInteger(capacity)||capacity<1||capacity<members.length)return 'Choose a whole number of seats that fits every selected member.';
    const t=model.teams.find(t=>t.id===item),values={name:data.name,focus:data.focus,capacity,members};
    model.teams.forEach(t=>t.members=t.members.filter(p=>!members.includes(p)));if(t)Object.assign(t,values);else model.teams.push({...values,id:'team-'+Date.now()});model.team=model.teams.find(t=>t.members.includes(currentId()))?.id||'';save();navigate('admin-teams',{state:'saved'});return true;
  }
  if(id==='team-remove'){model.teams=model.teams.filter(t=>t.id!==item);if(model.team===item)model.team='';save();navigate('admin-teams');return true;}
  if(id==='assign-all'){if(model.teams.reduce((n,t)=>n+t.capacity,0)<model.participants.length)return 'There aren’t enough seats for everyone. Add capacity before assigning teams.';model.teams=assignmentPreview();model.team=model.teams.find(t=>t.members.includes(currentId()))?.id||'';save();navigate('admin-teams',{state:'saved'});return true;}
  if(id==='project-edit'){const p=model.projects.find(p=>p.id===item);if(p)Object.assign(p,data);else model.projects.push({...data,id:'project-'+Date.now()});save();navigate('admin-projects',{state:'saved'});return true;}
  if(id==='project-remove'){model.projects=model.projects.filter(p=>p.id!==item);model.demos=model.demos.filter(d=>d.id!==item);if(model.project===item)model.project='';save();navigate('admin-projects');return true;}
  if(id==='question-edit'){const options=[data.option0,data.option1,data.option2];if(new Set(options.map(o=>o.trim().toLowerCase())).size<options.length)return 'Give each answer a different description.';const values={prompt:data.prompt,options,correct:data.correct},q=model.questions.find(q=>q.id===item);if(q)Object.assign(q,values);else model.questions.push({...values,id:'question-'+Date.now()});save();navigate('admin-quizzes',{state:'saved'});return true;}
  if(id==='question-remove'){model.questions=model.questions.filter(q=>q.id!==item);delete model.answers[item];model.quizIndex=0;save();navigate('admin-quizzes');return true;}
  if(id==='prize-edit'){const p=model.prizes.find(p=>p.id===item);if(p&&model.prizes.indexOf(p)<model.winners.length)return 'This prize has already been drawn. Add a new prize instead.';if(p)Object.assign(p,data);else model.prizes.push({...data,id:'prize-'+Date.now()});save();navigate('admin-raffle',{state:'saved'});return true;}
  if(id==='prize-remove'){const i=model.prizes.findIndex(p=>p.id===item);if(i<model.winners.length)return 'A drawn prize stays in the winner history.';model.prizes=model.prizes.filter(p=>p.id!==item);save();navigate('admin-raffle');return true;}
  if(id==='demo-edit'){
    if(!data.project)return 'Add a project before scheduling a demo.';
    if(model.demos.some(d=>d.id!==item&&d.id===data.project))return 'This project already has a demo.';
    if(data.time<model.event.start||data.time>=model.event.end)return 'Schedule the demo during the event.';
    if(model.demos.some(d=>d.id!==item&&d.time===data.time))return 'Another demo starts at this time.';
    const d=model.demos.find(d=>d.id===item),values={id:data.project,presenter:data.presenter,time:data.time};if(d)Object.assign(d,values);else model.demos.push(values);model.demos.sort((a,b)=>a.time.localeCompare(b.time));save();navigate('admin-demos',{state:'saved'});return true;
  }
  if(id==='demo-reorder'){const times=model.demos.map(d=>d.time),i=model.demos.findIndex(d=>d.id===item);if(i<0)return 'This demo is no longer available.';const [d]=model.demos.splice(i,1);model.demos.splice(Number(data.position),0,d);model.demos.forEach((d,i)=>d.time=times[i]);save();navigate('admin-demos',{state:'saved'});return true;}
  if(id==='demo-remove'){model.demos=model.demos.filter(d=>d.id!==item);save();navigate('admin-demos');return true;}
  return false;
}
