using FaithTechTorontoAiBuildEvent.Application.Events;

namespace FaithTechTorontoAiBuildEvent.Application.Scheduling;

public static class September9Reference
{
    public static ScheduleInput Create()
    {
        var rows = new (int Hour, int Minute, string Phase, string Name, string Screen, string Content)[] {
            (17, 0, "Gather", "Arrival and welcome information", "welcome", "Welcome to FaithTech Toronto AI Build Night at Stone Church. Grab a name tag, meet someone, and get ready to build together."),
            (17, 20, "Gather", "Welcome and opening prayer", "welcome", "Thank you for bringing your laptop and showing up to build for the Kingdom. Join the opening prayer: establish the work of our hands. Psalm 90:17. Demos begin at 20:30."),
            (17, 22, "Gather", "Tonight's outcome and milestones", "information", "Leave with a demo video you can share. Milestones: specs at 18:40, mocks and design at 19:20, building at 19:45, and demo recording at 20:10."),
            (17, 28, "Discover", "Introductions", "people", "Take about 40 seconds to share your name, what you make, and what is on your heart."),
            (17, 40, "Discover", "The 4D cycle", "information", "Discover: see clearly through the lens of Christ. Discern: choose wisely. Develop: co-create in sprints. Demonstrate: measure love through friendship compounded by time."),
            (17, 45, "Discover", "The five R's", "information", "Request: invite the Spirit into the work. Receive: wait and write down what comes. Review: synthesize a direction. Render: build and log it. Rejoice: give thanks for the good it serves."),
            (17, 50, "Discover", "Project one presentation", "information", "Listen to the first project presentation. Learn who it serves and what help is needed. Project choices and details are supplied by the administrators."),
            (17, 55, "Discover", "Project two presentation", "information", "Listen to the second project presentation, then consider where you can contribute. This presentation does not create project records or assign participants."),
            (18, 5, "Discern", "Lament, then discern", "teams", "Name what is broken and who it hurts before naming a feature. Consider Reject, Receive, Reimagine, or Create. Choose a team and project; selection stays open until 18:15."),
            (18, 10, "Discern", "Scope down", "projects", "Choose something you can demonstrate in a 90-second video by 20:30. Write the build in one sentence. Team and project selection remains open until 18:15."),
            (18, 15, "Develop", "Setup sprint and building guidance", "build", "Create a repository and confirm push access. Install the tools your project needs, confirm the agent-toolkit plugin is loaded, and generate agent instruction files. Keep repository and demo links with your project."),
            (18, 30, "Develop", "Write the mini-PRD", "build", "Write one page: who the project is for, the problem, and what working looks like. Hand the mini-PRD to the agent at 18:40."),
            (18, 40, "Develop", "Build the specs; pizza and four ideas", "build", "Use the requirements-engineer skill to produce L1 capabilities and L2 behaviors with Given-When-Then criteria. While it works, enjoy pizza and discuss four ideas: MCP, AGENTS.md, skills, and agentic workflows."),
            (19, 10, "Develop", "Read what the agent wrote", "build", "Receive and review the requirements. Correct mistakes before they become code. Write a mock prompt that clearly identifies HTML mocks as design artifacts."),
            (19, 20, "Develop", "Mocks and detailed design; stand up and network", "build", "Create mocks and detailed designs from the specs, including C4, class, and sequence diagrams. While the agent works, stand up, refill, change tables, and network by introducing two people who have not spoken."),
            (19, 35, "Develop", "Resolve the drift", "build", "Compare specs, designs, and mocks. Reconcile their contradictions and report each one before implementing production code."),
            (19, 45, "Develop", "Build incrementally", "build", "Use ATDD and the incremental implementation skill: failing acceptance test, code, green, commit. Build one vertical slice at a time, linked to an L2 requirement. At 20:00, thirty minutes remain: stop adding and start cutting scope for the demo."),
            (20, 10, "Demonstrate", "Record the demo; write your 60 seconds", "build", "Use the demo-video skill to record each executable application. While it renders, write your 60 seconds: the problem, who it serves, what the agent did, and what comes next. Prepare one speaker and have the video ready by 20:25."),
            (20, 30, "Demonstrate", "Team demos", "demos", "Demos start now. Share the video and a short explanation, allowing about three minutes per team. Celebrate what was made; no live coding is needed."),
            (20, 50, "Send", "Raffle", "raffle", "Thank you for coming and building. Join the raffle and celebrate the community. Prizes and eligibility are configured separately by the administrators."),
            (20, 55, "Send", "What's next, thanks, and closing prayer", "recap", "Keep your repository alive beyond tonight. Thank you, Stone Church. Join the closing prayer, take your demo video with you, and share what you built.")
        };
        var stages = rows.Select((row, index) => new StageInput(Guid.NewGuid(), row.Name, row.Phase, row.Screen, row.Content, null,
            Time(row.Hour, row.Minute), index + 1 < rows.Length ? Time(rows[index + 1].Hour, rows[index + 1].Minute) : Time(21))).ToArray();
        return new("America/Toronto", Time(17), Time(21), stages, new(Time(18, 5), Time(18, 15)), new(Time(20, 30), Time(20, 50)));
    }
    private static LocalTimeInput Time(int hour, int minute = 0) => new(new DateTime(2026, 9, 9, hour, minute, 0), -240);
}
