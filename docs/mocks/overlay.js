import { dialogContent, dialogMarkup } from './dialogs.js';
import { adminDialogContent } from './admin-dialogs.js';
import { adminActivityDialogContent } from './admin-activity-dialogs.js';
const overlay=document.querySelector('#overlay');
let opener,held,pending,dirty=false,submitting=false;
export function clean(){dirty=false;}
export function submittingForm(value){submitting=value;}
export function openDialog(id,item){
  if(!overlay.open)opener=document.activeElement;
  overlay.innerHTML=dialogMarkup(id,item,dialogContent(id,item)||adminDialogContent(id,item)||adminActivityDialogContent(id,item));
  if(overlay.innerHTML&&!overlay.open)overlay.showModal();
}
export function closeDialog(){overlay.close();overlay.innerHTML='';const q=new URLSearchParams(location.search);q.delete('dialog');history.replaceState({},'','?'+q);if(opener?.isConnected)opener.focus();}
export function requestLeave(action){
  if(!dirty||submitting){action();return;}
  if(pending)return;
  pending=action;held=overlay.open?overlay.firstElementChild:null;openDialog('unsaved');
}
export function cancelDialog(){
  if(pending){pending=null;if(held){overlay.replaceChildren(held);held=null;overlay.querySelector('input,textarea,button')?.focus();}else closeDialog();return;}
  requestLeave(()=>{clean();closeDialog();});
}
export function discard(){clean();const action=pending;pending=null;held=null;closeDialog();action?.();}
document.addEventListener('input',e=>{const f=e.target.closest('[data-form]');if(f&&!['review','clock','search','access','admin-login','quiz'].includes(f.dataset.form))dirty=true;});
overlay.addEventListener('cancel',e=>{e.preventDefault();cancelDialog();});
