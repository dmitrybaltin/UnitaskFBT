![FBT_Logo](.docs/fbt_icon.png)

# UnitaskFBT

UnitaskFBT is the second version of the [Functional Behavior Tree (FBT)](https://github.com/dmitrybaltin/FunctionalBT) project, designed for building sophisticated AI in Unity or C# projects.

Unlike classical FBT, this version uses asynchronous functions, allowing you to write compact, readable, and maintainable code for complex AI behaviors involving long-running actions.

## Why UnitaskFBT?

Building complex AI often requires sequences of actions that take several seconds to complete. For example, an NPC attack may involve:
- Pausing to aim
- Preparing to jump
- Jumping
- Hitting the target or playing a miss animation
- Chaining into combos
- Reacting to the player’s actions during the attack

Classical behavior trees require multiple state variables and constant checks to manage these sequences, which can make code messy and hard to debug.  
With **UnitaskFBT**, you can use async/await syntax to handle long-running actions naturally, keeping your code simple, readable, and efficient.

## Key Features

- Full async support – Perform long-running actions efficiently without extra flags or checks.
- Boolean node results – Nodes return true = Success or false = Fail. No Running state clutter, simplifying logic.
- Compact, readable code – Makes complex AI logic easier to understand and maintain.
- Inherited FBT advantages – Memory efficiency, performance, and debug-friendly behavior.

## Example of Usage

```csharp
    await npcBoard.Sequencer(c,                       //Sequencer node
        static async (b, c) => await b.FindTarget(),  //Action node realized as a delegate Func<NpcBoard, UniTask<bool>> 
        static async (b, c) => await b.Selector(c,    //Selector node
            static async (b, c) => await b.If(c,      //Conditional node 
                static b => b.TargetDistance < 1f,    //Condition
                static async (b, c) => await b.MeleeAttack()),
            static async (b, c) => await b.If(c,
                static b => b.TargetDistance < 3f,
                static async (b, c) => await b.RangeAttack()),  //RangeAttack() is the only continuous function in this BT allowing be in the running state
            static async (b, c) => await b.If(c,
                static b => b.TargetDistance < 8f,
                static async (b, c) => await b.Move()),
            static async (b, c) => await b.Idle()));
```

Notes: 
- Each tree node is an asynchronous function, not an object.
- The entire tree is also an asynchronous function, not a static object.
- The construction static async (b, c) => await … in each line may look verbose, but it’s a small price to pay for the huge advantages of an asynchronous behavior tree.
- If a node needs to continue execution in the next cycle, it does not return Running. Instead, it suspends itself (e.g., using await UniTask.Yield()). In the example above, this happens inside RangeAttack().
- Since there is no Running state, nodes simply return a boolean (bool) rather than a Status, which simplifies the logic.
- All nodes accept a CancellationToken for operation cancellation.
- No closures are used; only static delegates are employed, which avoids additional memory allocations.

For a detailed comparison between UnitaskFBT and a classical FBT, see this repository [FbtExample](https://github.com/dmitrybaltin/FbtExample)  

