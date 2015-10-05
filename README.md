C# implementations of some usefull data sctructures for .NET
 - [Priority queue](#priority-queue)
 - [Skip list](#skip-list)


##Priority queue
Heap-based generic concurrent [priority queue](http://en.wikipedia.org/wiki/Priority_queue) for .NET

>Priority queue is an abstract data type which is like a regular queue or 
>stack data structure, but where additionally each element has a "priority" 
>associated with it. In a priority queue, an element with high priority is 
>served before an element with low priority. If two elements have the same 
>priority, they are served according to their order in the queue.

###Features
- Generic
- Thread-safe using [ReaderWriterLockSlim](https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx)
- Performant
    - Take max item, Insertion, Removal - `O(N log N)`
- Resizable (queue grows and shrinks depending on the number of items)

#NuGet
- Install `PM> Install-Package ConcurrentDataStructures`
- [https://www.nuget.org/packages/ConcurrentDataStructures](https://www.nuget.org/packages/ConcurrentDataStructures//)

###Applications

- Bandwidth management
- [Discrete event simulation](http://en.wikipedia.org/wiki/Discrete_event_simulation)
- [Huffman coding](http://en.wikipedia.org/wiki/Huffman_coding)
- [Best-first search algorithms](http://en.wikipedia.org/wiki/Best-first_search)
- [ROAM triangulation algorithm](http://en.wikipedia.org/wiki/ROAM)
- [Dijkstra's algorithm](http://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)
- [Prim's Algorithm](http://en.wikipedia.org/wiki/Prim%27s_algorithm) for Minimum Spanning Tree

##Skip list
Generic concurrent [skiplist](https://en.wikipedia.org/wiki/Skip_list) for .NET

>This data structure makes random choices in arranging the entries in such
>a way that search and update times are O(log N) **on average**, where N is the number
>of entries in the list. Interestingly, the notion of average time complexity
>used here does not depend on the probability distribution of the keys in the input.
>Instead, it depends on the use of a random-number generator in the implementation
>of the insertions to help decide where to place the new entry.
>[Detailed overview] (https://msdn.microsoft.com/en-us/library/ms379573(VS.80).aspx#datastructures20_4_topic4) of skip list and a simple implementation.

###Features
 - Generic
 - Thread-safe using [ReaderWriterLockSlim](https://msdn.microsoft.com/en-us/library/system.threading.readerwriterlockslim(v=vs.110).aspx)
 - Performant
    - Take min or max item - `O(1)`
    - Insertion, Removal, Check if contains - `O(log N)`
    - Enumeration in order - `O(N)`
 - Additional operations
    - Get Floor and Clealing items - `O(log N)`
    - Get items in range `O(log N + K)` (where K is number of items in result)

###Applications

- Can be used as priority queue
- [Moving medians](https://en.wikipedia.org/wiki/Moving_average#Moving_median)

#License
Released under the MIT license.