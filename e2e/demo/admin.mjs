import { record } from './capture.mjs';
import { AdminAccessPage } from '../page-objects/admin-access-page.ts';
import { AdminEventsPage } from '../page-objects/admin-events-page.ts';
import { AdminEventEditorPage } from '../page-objects/admin-event-editor-page.ts';
import { AdminSchedulePage } from '../page-objects/admin-schedule-page.ts';
import { AdminRosterPage } from '../page-objects/admin-roster-page.ts';
import { AdminDemoPage } from './admin-demo-page.mjs';

const lines = [
  'Welcome to the Faith Tech event administrator. Hosts use this application to prepare an event, arrange its schedule, and manage participant access.',
  'The administrator account was provisioned locally. Signing in opens the protected host workspace. This recording uses the real API and an isolated SQL database.',
  'Our new event has been saved as a draft. Hosts can prepare its details before participants arrive. Publication is still under construction in this revision.',
  'Venue and waiting content have been saved. Reopening the event confirms these details survived navigation and came back from the server.',
  'The September ninth reference replaces the current schedule only after confirmation. It supplies the Toronto event timing and content as an editable starting point.',
  'Demo presentations have their own window, from eight thirty to eight fifty. Refreshing confirms that these times came back from the saved reference configuration.',
  'The optional Liturgy connection is off, as required for this September event. Core event setup remains independent of that companion application.',
  'The roster now contains Alex Morgan. A fresh entry code was issued once and is concealed in this recording. Hosts must share that code privately with its participant.',
  'The participant name has been updated. Deactivation provides a way to withdraw access, including revoking existing participant sessions.',
  'The roster shows this participant as inactive. Reactivating the entry restores eligibility to sign in, without reviving an old revoked session.',
  'The participant is active again after refreshing the roster. This completes a host setup journey through saved event details, scheduling, and participant access.',
  'The saved waiting message welcomes builders and invites them to meet their team. Hosts can edit this content for each event.',
  'Project selection has a separate ten minute window, from six oh five to six fifteen. It does not have to share the timing of demo presentations.',
  'The reference also saved twenty one editable stage entries, beginning with arrival and welcome information. Each stage carries its own timing and instructions.',
];
export async function admin(directory, environment) {
  return record('admin', directory, lines, async ({ page, say, expect }) => {
    const access = new AdminAccessPage(page), events = new AdminEventsPage(page), editor = new AdminEventEditorPage(page);
    const schedule = new AdminSchedulePage(page), roster = new AdminRosterPage(page);
    const framing = new AdminDemoPage(page);
    await page.addInitScript(() => {
      const add = () => { const style = document.createElement('style'); style.textContent = 'dialog input[readonly]{-webkit-text-security:disc!important}'; document.head.append(style); };
      if (document.head) add(); else document.addEventListener('DOMContentLoaded', add, { once: true });
    });
    await page.goto('/admin/sign-in'); await access.expectSignIn(); await say(0, 'Host workspace');
    await access.signIn('demo-host', environment.password); await events.open(); await events.expectEmpty(); await say(1, 'Authenticated access');
    const title = 'Toronto AI Build Event — host demo';
    await events.createDraft(title); await events.expectDraft(title); await say(2, 'Create a draft');
    await events.openEvent(title); await editor.expectDraft(title);
    await editor.editContent(title, 'Toronto gathering space', 'Welcome, builders. Bring your curiosity and meet your team.');
    await editor.save(); await editor.expectSaved(); await editor.returnToEvents(); await events.openEvent(title);
    await editor.expectContent('Toronto gathering space', 'Welcome, builders. Bring your curiosity and meet your team.');
    await framing.showField('Venue name'); await say(3, 'Saved venue and content');
    await framing.showField('Waiting content'); await say(11, 'Waiting message');
    await schedule.open(); await schedule.requestReference(); await say(4, 'Reference confirmation');
    await schedule.confirmReference(); await schedule.expectSaved(); await page.reload();
    await schedule.expectWindow('selection', '2026-09-09T18:05', '2026-09-09T18:15');
    await framing.showField('Selection start'); await say(12, 'Selection window');
    await schedule.expectWindow('demo presentation', '2026-09-09T20:30', '2026-09-09T20:50');
    await framing.showField('Demo presentation start'); await say(5, 'Presentation window');
    await framing.showStages(); await say(13, 'Stage content');
    await schedule.returnToSettings(); await editor.expectCompanion(false);
    await framing.showField('Use Liturgy'); await say(6, 'Optional connection off');
    await roster.open(); await roster.expectEmpty(); await roster.add('Alex Morgan'); await roster.takeCode('Alex Morgan');
    await roster.expectParticipants('Alex Morgan', 1); await say(7, 'Issue participant access');
    await roster.rename('Alex Morgan', 'Alex Morgan — Builder'); await roster.expectParticipants('Alex Morgan — Builder', 1); await say(8, 'Update a participant');
    await roster.deactivate('Alex Morgan — Builder'); await roster.expectStatus('Alex Morgan — Builder', 'Inactive'); await say(9, 'Deactivate access');
    await roster.reactivate('Alex Morgan — Builder'); await roster.expectStatus('Alex Morgan — Builder', 'Active');
    await page.reload(); await roster.expectStatus('Alex Morgan — Builder', 'Active'); await say(10, 'Restore eligibility');
  }, { baseURL: environment.url });
}
