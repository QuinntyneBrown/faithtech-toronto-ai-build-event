import { model, save } from './data.js';
export const minutes = t => Number(t.split(':')[0])*60+Number(t.split(':')[1]);
const time = n => `${String(Math.floor(n/60)).padStart(2,'0')}:${String(n%60).padStart(2,'0')}`;
export async function submitAdmin(id,data,item,navigate) {
  if(id==='admin-login'){if(data.email!=='host@example.com'||data.password!=='host-demo')return 'Those details don’t match an administrator account.';model.admin=true;save();navigate('admin-events');return true;}
  if(id==='event-settings'){
    if(data.end<=data.start)return 'The event must end after it starts.';
    if(data.newEvent!=='true'&&model.stages.some(s=>s.start<data.start||s.end>data.end))return 'The event times must contain every scheduled stage. Adjust the stages first.';
    let logo=model.event.logo;
    if(data.logo?.size){if(data.logo.size>2*1024*1024)return 'Choose a logo smaller than 2 MB.';logo=await new Promise((resolve,reject)=>{const r=new FileReader();r.onload=()=>resolve(r.result);r.onerror=reject;r.readAsDataURL(data.logo);});}
    if(data.newEvent==='true'){model.archivedEvents||=[];model.archivedEvents.push({...model.event});model.stages=[];model.winners=[];model.team='';model.project='';}
    model.event={...model.event,name:data.name,date:data.date,start:data.start,end:data.end,timezone:data.timezone,venue:data.venue,address:data.address,liturgy:data.liturgy==='on',logo};save();navigate('admin-settings',{state:'saved'});return true;
  }
  if(id==='participant-edit') {
    if(model.participants.some(p=>p.id!==item&&p.email.toLowerCase()===data.email.toLowerCase()))return 'This email is already on the guest list.';
    const existing=model.participants.find(p=>p.id===item);if(existing)Object.assign(existing,data);else model.participants.push({...data,id:'person-'+Date.now(),skills:data.role,interests:''});save();navigate('admin-participants',{state:'saved'});return true;
  }
  if(id==='participant-remove'){model.participants=model.participants.filter(p=>p.id!==item);model.teams.forEach(t=>t.members=t.members.filter(id=>id!==item));save();navigate('admin-participants');return true;}
  if(id==='stage-edit') {
    if(data.end<=data.start)return 'The stage must end after it starts.';
    if(data.start<model.event.start||data.end>model.event.end)return 'Keep the stage within the event start and end times.';
    if(model.stages.some(s=>s.id!==item&&data.start<s.end&&data.end>s.start))return 'These times overlap another stage. Choose an available time.';
    const s=model.stages.find(s=>s.id===item);if(s)Object.assign(s,data);else model.stages.push({...data,id:'stage-'+Date.now()});model.stages.sort((a,b)=>a.start.localeCompare(b.start));save();navigate('admin-schedule',{state:'saved'});return true;
  }
  if(id==='stage-remove'){model.stages=model.stages.filter(s=>s.id!==item);save();navigate('admin-schedule');return true;}
  if(id==='stage-reorder'){const stages=[...model.stages],i=stages.findIndex(s=>s.id===item);if(i<0)return 'This stage is no longer available.';const [s]=stages.splice(i,1);stages.splice(Number(data.position),0,s);let start=minutes(model.event.start);const adjusted=stages.map(s=>{const duration=minutes(s.end)-minutes(s.start),result={...s,start:time(start),end:time(start+duration)};start+=duration;return result;});if(start>minutes(model.event.end))return 'The stages no longer fit within the event.';model.stages=adjusted;save();navigate('admin-schedule',{state:'saved'});return true;}
  return false;
}
