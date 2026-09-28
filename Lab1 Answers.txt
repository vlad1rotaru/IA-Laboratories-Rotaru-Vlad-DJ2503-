1)A Finite State Machine (FSM) is a system used to control an object's behavior by dividing it into a limited number of states. The object changes from one state to another when specific conditions are met. For example, an enemy can have states such as Patrol, Chase, Attack, and Flee.
2)A state represents what the NPC is currently doing, such as Patrol or Chase.
A transition is the condition or rule that causes the NPC to change from one state to another.
3)Basic enemy behavior usually requires:
Patrol - the enemy moves around an area.
Chase - the enemy follows the player when detected.
Attack - the enemy attacks when close enough to the player.
Flee - the enemy runs away when necessary.
4)Because the player can be close to the NPC but behind it or behind an obstacle. Distance only tells us how far away the player is, not whether the NPC actually has a clear view of them.
5)It can be implemented using an angle between the NPC's forward direction and the direction toward the player. If the angle is within a certain limit, for example ±90° for a 180° field of view, the player is inside the NPC's vision.
6)A Raycast checks whether there is a clear line of sight between the NPC and the player. It can detect obstacles such as walls, preventing the NPC from seeing the player through them.
7)A 180° field of view allows the NPC to detect objects mainly in front of it.
360° detection allows the NPC to detect objects in every direction around it, regardless of which way it is facing.
8)Advantages: FSMs are simple, easy to understand, easy to implement, and good for basic enemy behavior.
Disadvantages: They can become complicated when there are many states and transitions, and they are less flexible for very complex AI behavior.
9)The system should use a priority order for transitions. The transition with the highest priority should be selected. For example, if an enemy can both Chase and Attack, Attack could have higher priority when the player is close enough.
10)A new enemy type could be created with its own FSM and states, or the existing FSM could be extended with states and transitions specific to that enemy. For example, a ranged enemy could have Patrol, Chase, Shoot, and Retreat states.