![FBT_Logo](.docs/fbt_icon.png)

# UnitaskFBT

UnitaskFBT(or UFBT) is the second generation of the [Functional Behavior Tree (FBT)](https://github.com/dmitrybaltin/FunctionalBT) project, designed for building sophisticated AI in Unity or C# projects.

Unlike classical FBT, UFBT uses asynchronous functions, allowing you to write compact, readable, and maintainable code for complex AI behaviors involving long-running actions.

## Why UnitaskFBT?

Building complex AI often requires sequences of actions that take several seconds to complete. For example, an NPC attack may involve:
- Pausing to aim
- Preparing to jump
- Jumping
- Hitting the target or playing a miss animation
- Chaining into combos
- Reacting to the player’s actions during the attack

Classical BT require state variables and their checks to manage these sequences, which can make code messy and hard to debug.  

With **UnitaskFBT**, you can use async/await syntax to handle long-running actions naturally, keeping your code simple, readable, and efficient.

## Key Features

- Full async support – Perform long-running actions efficiently without extra flags or checks.
- Boolean node results – Nodes return true = Success or false = Fail. No Running state clutter, simplifying logic.
- Compact, readable code – Makes complex AI logic easier to understand and maintain.
- Inherited FBT advantages – Memory efficiency, performance, and debug-friendly behavior.

## Example of Usage

```csharp
    await npcBoard.Sequencer( //Sequencer node
        static b => b.FindTarget(), //Action node realized as a delegate Func<NpcBoard, UniTask<bool>> 
        static b => b.Selector(     //Selector node
            static b => b.If(       //Conditional node 
                static b => b.TargetDistance < 1f,  //Condition
                static b => b.MeleeAttack()),       //Action
            static b => b.If(
                static b => b.TargetDistance < 3f,
                static b => b.RangeAttack()),       //Continuous function that can be "running"
            static b => b.If(
                static b => b.TargetDistance < 8f,
                static b => b.Move()),
            static b => b.Idle()));
```

Notes: 
- Each tree node is an asynchronous function, not an object.
- The entire tree is also an asynchronous function, not a static object.
- The construction static async (b, c) => await … in each line may look verbose, but it’s a small price to pay for the huge advantages of an asynchronous behavior tree.
- If a node needs to continue execution in the next cycle, it does not return Running. Instead, it suspends itself (e.g., using await UniTask.Yield()). In the example above, this happens inside RangeAttack().
- Since there is no Running state, nodes simply return a boolean (bool) rather than a Status, which simplifies the logic.
- No closures are used; only static delegates are employed, which avoids additional memory allocations.
- If CancellationToken required it can be added to Blackboard object

For a detailed comparison between UnitaskFBT and a classical FBT, see this repository [FbtExample](https://github.com/dmitrybaltin/FbtExample)  

## Installation

You can install Functional Behavior Tree (FBT) in Unity using one of the following methods:

### 1. Install from GitHub as a Unity Package
1. Open your Unity project.
1. Go to **Window → Package Manager**.
1. Click the **+** button in the top-left corner and choose **Add package from git URL...**.
1. Enter the URL: https://github.com/dmitrybaltin/UnitaskFBT.git
1. Click **Add**. The package will be imported into your project.

### 2. Install via OpenUPM

1. Open your Unity project.
2. Go to **Edit → Project Settings → Package Manager → Scoped Registries**
3. Add a new registry for OpenUPM:
   - **Name:** OpenUPM
   - **URL:** `https://package.openupm.com`
   - **Scopes:** `com.baltin`
4. Open the **Package Manager** (`Window → Package Manager`).
5. Click **+ → Add package from git URL...** (or search in the registry if the package appears) and enter: **com.baltin.ufbt**

### 3. Install as a Git Submodule

1. Navigate to your Unity project folder in a terminal.
1. Run
```
git submodule add https://github.com/dmitrybaltin/UnitaskFBT.git Packages/UnitaskFBT
git submodule update --init --recursive
```

## Dependencies

This project depends on **UniTask**, which serves as the foundation because it is the most popular and highly optimized async solution for Unity.

If you want to avoid this dependency, you can manually replace `UniTask` with `Task`, `ValueTask`, or `Awaitable` in the source; it is not a difficult - the project code is ~200 lines - it is a pattern, not a lib.

Unfortunately, it is impossible in C# to implement a single, fully generic async solution that works for all Task-like or awaitable types at once — otherwise I would have done it here.

If there is demand from users, I will provide additional versions of the async FBT based on `Task`, `ValueTask`, or a custom awaitable.

## Dependency

This project depends on **UniTask**, which serves as the foundation because it is the most popular and highly optimized async solutions for Unity.

If you want to avoid this dependency, you can manually replace `UniTask` with `Task`, `ValueTask`, or a custom awaitable type in the source. (All the codebase of teh project is ~200 lines including comments).

Unfortunately, it's not practical to implement a single, fully generic async solution that works with all Task-like types at once. 

If there is enough demand, I may provide additional versions of the library based on `Task`, `ValueTask`, or custom awaitables.


# Async Functional Behavior Tree Philosophy

My initial idea was to create a debuggable and simple behavior tree using the functional programming features of C#.  
That’s how [FunctionalBT](https://github.com/dmitrybaltin/FunctionalBT) was born. By the way, because of its simplicity, the tree turned out to be not only easy to understand, but also zero-allocated and highly performant.  

However, all classic behavior tree implementations — including mine — share one inherent complexity: the **Running ** return value.

When a node returns Running, it should continue execution on the next tick. But a classic tree always starts from the root, so intermediate nodes may intercept control, and execution never returns to the previously running node automatically. The responsibility of synchronizing node execution falls to the AI developer, who typically introduces flags or state variables. For complex AI the synchronization code grows exponentially and becomes a combinatorial problem.

Take, for example, an NPC attack. It is not a single short action but a sequence lasting several seconds, with multiple branches: preparation, attack animation, hit reaction, miss reaction, and so on.
You may also have different attack types, such as melee and ranged. Ideally, each such complex sequence should be represented as one branch of the tree and executed until it completes.
But in a classic tree this requires manually storing the current action and its stage. In practice, this means you end up building a kind of a state machine again — the very thing behavior trees were supposed to replace.

**How to solve it?**

C# already has asynchronous functions, designed for long-running actions. Combining them with the behavior tree concept solves the synchronization problem:
- less boilerplate code,
- easier AI development,
- and cleaner, more maintainable logic.

This is the goal of this project. 
