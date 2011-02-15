namespace GabeSoft.Common

/// Functional queue.
type 'a Queue = Queue of 'a list * 'a list
exception EmptyQueueException

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Queue = 
   let queue = function
   | [], r  -> Queue (List.rev r, [])
   | f, r   -> Queue (f, r)

   /// Returns the length of the queue.
   let length (Queue (f, r)) = List.length f + List.length r

   /// Creates an empty queue.
   let empty<'a> : 'a Queue = queue ([], [])

   /// Returns true if the queue contains no items, false otherwise.
   let isEmpty = function
   | Queue ([], _)   -> true
   | _               -> false

   /// Adds an item to the end of the queue.
   let snoc x (Queue (f, r)) = queue (f, x::r)
   
   /// Returns the first item in the queue.
   let head = function
   | Queue ([], _)   -> raise EmptyQueueException
   | Queue (x::_, _) -> x
   
   /// Returns the queue with the first item removed.
   let tail = function
   | Queue ([], _)   -> raise EmptyQueueException
   | Queue (_::f, r) -> queue (f, r)

   /// Converts the given list into a queue.
   let rec ofList s = 
      let rec loop q = function
      | []     -> q
      | h::t   -> loop (snoc h q) t
      loop empty<_> s

   /// Converts the given queue into a list.
   let rec toList q =
      let rec loop q acc = 
         if isEmpty q 
         then acc 
         else loop (tail q) ((head q)::acc)
      loop q [] |> List.rev         

/// Lazy functional queue.
type 'a QueueL = QueueL of 'a list * 'a list Stream * int * 'a list * int
exception EmptyQueueLException

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module QueueL =
   let rec private queue = function
   | ([], m, lenfm, r, lenr)  -> 
      if Stream.isEmpty m 
      then QueueL (r, Stream.empty<_>, lenr, [], 0)
      else make (Stream.head m, Stream.tail m, lenfm, r, lenr)
   | (f, m, lenfm, r, lenr)   -> make (f, m, lenfm, r, lenr)
   and private make (f, m, lenfm, r, lenr) =
      if lenr <= lenfm 
      then QueueL (f, m, lenfm, r, lenr)
      else QueueL (f, Stream.snoc (List.rev r) m, lenfm + lenr, [], 0)

   /// Returns the length of the queue.
   let length (QueueL (_, _, lenfm, _, lenr)) = lenfm + lenr

   /// Creates an empty queue.
   let empty<'a> = QueueL ([], Stream.empty<'a list>, 0, [], 0)

   /// Returns true if the queue contains no items, false otherwise.
   let isEmpty = function
   | QueueL ([], _, _, _, _)   -> true
   | _                        -> false

   /// Adds an item to the end of the queue.
   let snoc x (QueueL (f, m, lenfm, r, lenr)) =
      queue (f, m, lenfm, x::r, lenr + 1)

   /// Returns the first item in the queue.
   let head = function
   | QueueL ([], _, _, _, _)   -> raise EmptyQueueLException
   | QueueL (x::f, _, _, _, _) -> x

   /// Returns the queue with the first item removed.
   let tail = function
   | QueueL ([], _, _, _, _)            -> raise EmptyQueueLException
   | QueueL (x::f, m, lenfm, r, lenr)   -> queue (f, m, lenfm - 1, r, lenr)

   /// Converts the given list into a queue.
   let rec ofList s = 
      let rec loop q = function
      | []     -> q
      | h::t   -> loop (snoc h q) t
      loop empty<_> s

   /// Converts the given queue into a list.
   let rec toList q =
      let rec loop q acc = 
         if isEmpty q 
         then acc 
         else loop (tail q) ((head q)::acc)
      loop q [] |> List.rev
         
