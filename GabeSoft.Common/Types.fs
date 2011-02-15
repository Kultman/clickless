namespace GabeSoft.Common

open System

/// Binary tree
type 'a btree = 
   /// Binary tree leaf
   | BLeaf 
   /// Binary tree node
   | BNode of 'a btree * 'a * 'a btree
/// Binary tree exception
exception BTreeExc of string

/// Generic tree
type 'a gtree = 
   /// Geneneric tree node
   | GNode of 'a * 'a gtree list