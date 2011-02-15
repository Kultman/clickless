namespace GabeSoft.Common

open System
open System.Xml
open System.Xml.Linq
open System.Web
open System.Text.RegularExpressions

module Core =
   let floatEx x = 
      match box x with
      | :? int as i -> float i
      | :? int16 as i -> float i
      | :? int64 as i -> float i
      | :? float as i -> i
      | :? string as i -> float i
      | _ -> failwithf "cannot convert %s to float" (x.GetType().Name)

   let uncurry f = fun a b -> f (a, b)
   let curry f = fun (a, b) -> f a b
   let flip f = fun a b -> f (b, a)

module Url = 
   let helper url : string -> 'a -> string when 'a :> seq<string * string> =
      let addEndSlash (url : string) = url.TrimEnd('/') + "/"
      let remStartSlash (path : string) = path.TrimStart('/')
      fun path props ->
         seq { yield addEndSlash url
               yield remStartSlash path
               if props |> Seq.length > 0 then
                  yield "?"
                  yield seq { for key, value in props do 
                                 yield sprintf "%s=%s" key (HttpUtility.UrlEncode(value:string)) }
                        |> String.concat "&" }
         |> String.concat ""

module Xml =
   let xname nspace name = 
      if String.IsNullOrEmpty(nspace) 
      then XName.Get(name) 
      else XName.Get(name, nspace)
   let xnattr nspace name (elem: XElement) = elem.Attribute(xname nspace name).Value
   let xattr<'a> = xnattr ""
   let xnelem nspace name (elem: XContainer) = 
      let e = elem.Element(xname nspace name)
      if e = null then 
         try printfn "element %s not found on parent %s" name (elem :?> XElement).Name.LocalName
         with e2 -> printfn "element %s not found" name
         raise (new NullReferenceException(name))
      else e
   let xelem<'a> = xnelem ""
   let xvalue (elem: XElement) = elem.Value
   let xnelems nspace name (elem: XContainer) = elem.Elements(xname nspace name)
   let xelems<'a> = xnelems ""
   let xnnested nspace (path : string) (elem: XContainer) =
      path.Split([|'/'|], StringSplitOptions.RemoveEmptyEntries)
      |> Seq.fold (fun (xn : XContainer) name -> xnelem nspace name xn :> XContainer) elem
      :?> XElement
   let xnested<'a> = xnnested ""

module SeqPat =
   let rec (|SeqCons|SeqNil|) (s:seq<'a>) =
       match s with
       | :? LazyList<'a> as l ->
           match l with
           | LazyList.Cons(a,b) -> SeqCons(a,(b :> seq<_>))
           | LazyList.Nil -> SeqNil
       | _ -> (|SeqCons|SeqNil|) (LazyList.ofSeq s :> seq<_>)

module Seq =
   /// Splits the given sequence into a collection of arrays of length n.
   let split n (l : seq<_>) =
      use en = l.GetEnumerator()
      seq { while en.MoveNext() do
               let count = ref 1
               yield [| yield en.Current
                        while !count < n && en.MoveNext() do
                              count := !count + 1
                              yield en.Current |] }

   /// Returns a new collection that contains only the elements
   /// for which the given predicate returns true.
   let filteri f (l : seq<_>) =
      use en = l.GetEnumerator()
      let count = ref 0
      seq { while en.MoveNext() do
               if f !count en.Current then yield en.Current 
               count := !count + 1 }

   /// Returns a new collection that contains n elements given by the specified function.
   let gen n f = 
      Seq.unfold (fun x -> 
         if x < n then Some (f x, x + 1) else None) 0

   /// Returns an array containing averages of each column in the input arrays.
   /// The input arrays are assumed to be of same length.
   let averageCol (items : seq<'a array>) =
      let m = items |> Seq.length         
      let n = Seq.head items |> Seq.length
      let sum = [| for i in 1 .. n -> 0.0 |]
      if m = 0 then sum else
      use e = items.GetEnumerator()
      while e.MoveNext() do
         let r = e.Current
         r |> Array.iteri (fun i x -> sum.[i] <- sum.[i] + Core.floatEx x)
      sum |> Array.map (fun s -> s / float m) 

   /// Returns an array composed of the greatest elements of each column in the input arrays.
   /// The input arrays are assumed to be of same length.
   let maxCol (items : seq<'a array>) = 
      if items |> Seq.length = 0 then raise (new ArgumentException("The input sequence was empty."))
      let high = Seq.head items |> Array.copy
      use e = items.GetEnumerator()
      while e.MoveNext() do
         let r = e.Current
         r |> Array.iteri (fun i x -> if x > high.[i] then high.[i] <- x)
      high

   /// Returns an array composed of the lowest elements of each column in the input arrays.
   /// The input arrays are assumed to be of same length.
   let minCol (items : seq<'a array>)  =
      if items |> Seq.length = 0 then raise (new ArgumentException("The input sequence was empty."))
      let low = Seq.head items |> Array.copy
      use e = items.GetEnumerator()
      while e.MoveNext() do
         let r = e.Current
         r |> Array.iteri (fun i x -> if x < low.[i] then low.[i] <- x)
      low      

module List =
   /// Creates a list of one element.
   let ret x = [x]

   /// Take the specified number of items from the list
   /// and return them along with the remaining list.
   let take n l = 
      let rec iter n acc = function
         | l when n = 0 -> Some (List.rev acc, l)
         | h :: t -> iter (n-1) (h::acc) t
         | [] -> None
      iter n [] l

   /// Remove all items that satisfy the specified predicate
   /// from the beginning of the list.
   let trimStart pred l =
      let rec iter = function
         | h::t when pred h -> iter t
         | _ as l -> l
      iter l

   /// Variant of fold that uses the first element as the initial state.
   /// The input list is assumed to contain at least one element.
   let fold1 f = function
   | h::t -> List.fold f h t
   | _    -> failwith "the input list must contain at least one element"

   /// Variant of foldBack that uses the last element as the initial state.
   /// The input list is assumed to contain at least one element.
   /// TODO: Make tail recursive.
   let rec foldBack1 f = function
   | [h]    -> h
   | h::t   -> f h (foldBack1 f t)
   | _      -> failwith "the input list must contain at least one element"

   /// Splits the collection into two collections, for which the given
   /// predicate returns true and false respectively.
   let partitioni f l = 
      let rec loop i aacc bacc = function
      | []     -> (List.rev aacc, List.rev bacc)
      | h::t   -> if f i h 
                  then loop (i + 1) (h::aacc) bacc t
                  else loop (i + 1) aacc (h::bacc) t
      loop 0 [] [] l

   /// Randomly shuffles the elements of the given list.
   let shuffle<'a> : 'a list -> 'a list= 
      let rand = new Random()
      fun s ->
         let xs = List.toArray s
         for i in xs.Length .. -1 .. 1 do
            let j = rand.Next(0, i)
            let x = xs.[j]
            xs.[j] <- xs.[i - 1]
            xs.[i - 1] <- x
         Array.toList xs

   /// Builds a new list whose elements are the same as those of the input
   /// list except for the last which is the result of applying the given
   /// function to the last element in the list.
   let mapLast f s = 
      let rec loop acc = function
      | []     -> List.rev acc
      | h::[]  -> List.rev ((f h)::acc)
      | h::t   -> loop (h::acc) t
      loop [] s

   /// Builds a new list whose elements are the same as those of the input
   /// list except for the first which is the result of applying the given
   /// function to the first element in the list.
   let mapFirst f = function
   | []     -> []
   | h::t   -> (f h)::t

module Option =
   /// Creates a System.Nullable object from the given option.
   let toNullable = function
      | Some v -> System.Nullable(v)
      | None   -> System.Nullable()
      
module Tree =
   let private rev = List.rev
   let private exists = List.exists

   let private splitChildren (s:string) =
      let rec find_paren_pos i open_pos open_cnt close_cnt acc =
         if i = s.Length then acc else
         let find_next = find_paren_pos (i + 1)
         match s.[i] with
         | '(' -> if open_cnt > 0
                  then find_next open_pos (open_cnt + 1) close_cnt acc
                  else find_next i 1 0 acc
         | ')' -> if open_cnt = close_cnt + 1
                  then find_next -1 0 0 ((open_pos, i)::acc)
                  else find_next open_pos open_cnt (close_cnt + 1) acc
         | _   -> find_next open_pos open_cnt close_cnt acc
      let is_between_pos i pos = exists (fun (s, e) -> s < i && i < e) pos
      let rec find_split_pos i pos acc =
         if i = s.Length then acc else
         let find_next = find_split_pos (i + 1) pos
         match s.[i] with
         | ',' -> if is_between_pos i pos
                  then find_next acc
                  else find_next (i::acc)
         | _   -> find_next acc
      let rec split i acc = function
         | []   -> acc
         | [a]  -> (s.Substring(a + 1, s.Length - a - 1))::(s.Substring(i, a - i))::acc
         | a::l -> split (a + 1) (s.Substring(i, a - i)::acc) l

      let paren_pos = rev (find_paren_pos 0 -1 0 0 [])
      let comma_pos = rev (find_split_pos 0 paren_pos [])
      let children = rev (split 0 [] comma_pos)
      match children with
      | [] -> [s]
      | _  -> children

   let rec treeOfString emptyfn fullfn s =
      let tree_pat = new Regex("([^()]+)\\s*(\\((.*)\\))?")
      let m = tree_pat.Match(s)
      let info = m.Groups.[1].Value
      let has_children = m.Groups.[2].Value.Length > 0
      let children = m.Groups.[3].Value
 
      if children = null || children.Trim().Length = 0 then
         emptyfn has_children info 
      else
         let clean = splitChildren children
                        |> List.map (fun c -> c.Trim())
                        |> List.map (fun c -> treeOfString emptyfn fullfn c)
         fullfn info clean

