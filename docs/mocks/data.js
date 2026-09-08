export const initial = {
  event: { name: 'FaithTech Toronto AI Build Event', date: '2026-09-09', start: '18:00', end: '21:00', timezone: 'America/Toronto', venue: 'Toronto community venue', address: 'Downtown Toronto · Sample location', logo: '', liturgy: false, teamMode: 'self', proposals: true },
  profile: { name: 'Alex Morgan', role: 'Product designer', bio: 'Making useful things with thoughtful people.', skills: 'Design, accessibility, prototyping', interests: 'Community care, responsible AI' },
  participants: [
    { id: 'alex', name: 'Alex Morgan', role: 'Product designer', skills: 'Design, accessibility', interests: 'Community care', email: 'alex@example.com', code: 'BUILD26' },
    { id: 'sarah', name: 'Sarah Chen', role: 'Software developer', skills: 'Angular, AI, TypeScript', interests: 'Community care', email: 'sarah@example.com', code: 'BUILD26' },
    { id: 'marcus', name: 'Marcus Williams', role: 'Community organizer', skills: 'Research, facilitation', interests: 'Food security', email: 'marcus@example.com', code: 'BUILD26' },
    { id: 'priya', name: 'Priya Patel', role: 'Data scientist', skills: 'Python, responsible AI', interests: 'Accessibility', email: 'priya@example.com', code: 'BUILD26' },
    { id: 'daniel', name: 'Daniel Kim', role: 'Full-stack developer', skills: '.NET, Angular', interests: 'Neighbourhood connection', email: 'daniel@example.com', code: 'BUILD26' },
    { id: 'grace', name: 'Grace Okafor', role: 'UX researcher', skills: 'Interviews, service design', interests: 'Community care', email: 'grace@example.com', code: 'BUILD26' }
  ],
  teams: [{ id: 'north', name: 'North Star', focus: 'Community care', members: ['sarah', 'marcus'], capacity: 5 }, { id: 'open', name: 'Open Table', focus: 'Food security', members: ['priya', 'daniel'], capacity: 5 }, { id: 'neighbour', name: 'Good Neighbour', focus: 'Connection & belonging', members: ['grace'], capacity: 5 }],
  projects: [
    { id: 'care', name: 'CareLink', category: 'COMMUNITY CARE', description: 'Help neighbours find the support they need, when they need it.', problem: 'Finding local support can feel overwhelming. Make it easier for someone to take a first step toward help.', outcome: 'A welcoming, accessible guide to nearby community resources.', repository: 'https://example.com/carelink-repository', demo: 'https://example.com/carelink-demo', liturgy: 'https://example.com/liturgy/carelink', team: 'North Star' },
    { id: 'table', name: 'Open Table', category: 'FOOD SECURITY', description: 'Connect surplus food with the people and places that can use it.', problem: 'Good food goes unused while community kitchens need supplies.', outcome: 'A simple way to match offers with local needs.', repository: 'https://example.com/open-table-repository', demo: 'https://example.com/open-table-demo', liturgy: '', team: 'Open Table' },
    { id: 'hello', name: 'Hello, Neighbour', category: 'BELONGING', description: 'Make the first hello a little easier for newcomers to Toronto.', problem: 'Moving to a new city can be lonely.', outcome: 'Help newcomers discover welcoming local gatherings.', repository: '', demo: '', liturgy: '', team: 'Good Neighbour' }
  ],
  stages: [
    { id: 'welcome', name: 'Welcome', start: '18:00', end: '18:15', screen: 'welcome', content: 'Bring your curiosity. Build something that serves someone.', link: '' },
    { id: 'discover', name: 'Discover & form teams', start: '18:15', end: '18:35', screen: 'teams', content: 'Meet your team. Listen first and choose a problem that matters.', link: '' },
    { id: 'discern', name: 'Discern & choose a project', start: '18:35', end: '18:50', screen: 'projects', content: 'Choose one useful outcome you can demonstrate tonight.', link: '' },
    { id: 'develop', name: 'Develop', start: '18:50', end: '20:15', screen: 'build', content: 'Start small. Test an idea. Learn from someone who might use it.', link: '' },
    { id: 'quiz', name: 'Community quiz', start: '20:15', end: '20:25', screen: 'quiz', content: 'A quick pause to learn together.', link: '' },
    { id: 'demos', name: 'Demo & celebrate', start: '20:25', end: '20:50', screen: 'demos', content: 'Share what you learned, not just what you made.', link: '' },
    { id: 'raffle', name: 'Raffle & closing', start: '20:50', end: '21:00', screen: 'raffle', content: 'Celebrate the people who made tonight possible.', link: '' }
  ],
  questions: [{ id: 'q1', prompt: 'What is the best first step when building for a community?', options: ['Choose a framework', 'Listen to people affected by the problem', 'Build every feature'], correct: '1' }, { id: 'q2', prompt: 'How should we use an AI-generated answer?', options: ['Check it against reliable information', 'Assume it is always accurate', 'Publish it immediately'], correct: '0' }],
  prizes: [{ id: 'book', name: 'The builder’s bookshelf', description: 'A collection of books for curious builders.' }, { id: 'coffee', name: 'Coffee & conversation', description: 'A coffee gift card to keep the conversations going.' }],
  demos: [{ id: 'care', presenter: 'Sarah Chen', time: '20:25' }, { id: 'table', presenter: 'Priya Patel', time: '20:32' }, { id: 'hello', presenter: 'Grace Okafor', time: '20:39' }],
  messages: [{ from: 'sarah', text: 'Hi Alex! I saw you’re interested in community care. Want to build together?', time: '18:12' }],
  team: '', project: '', winners: [], answers: {}, quizIndex: 0, signedIn: false, admin: false, clock: '17:42', followClock: false
};
export let model;
try { model = JSON.parse(sessionStorage.getItem('faithtech-mock')) || structuredClone(initial); } catch { model = structuredClone(initial); }
export const save = () => sessionStorage.setItem('faithtech-mock', JSON.stringify(model));
export const reset = () => { Object.keys(model).forEach(k=>delete model[k]); Object.assign(model, structuredClone(initial)); save(); };
