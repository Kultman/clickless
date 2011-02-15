namespace GabeSoft.Drawing

open System
open System.Drawing.Drawing2D

/// Transformation functions
module Trans =
   let private degToRad d = (Math.PI * d) / 180.
   let private radToDeg r = (r * 180.) / Math.PI   

   let private sinDeg d = sin (degToRad d)
   let private cosDeg d = cos (degToRad d)

   /// The identity transformation.
   let identity : transformation = { m11=1.; m12=0.; m13=0.; m21=0.; m22=1.; m23=0. }  
   
   /// Applies a transformation to a point.
   let apply = 
      fun ({ m11=a; m12=b; m13=c; m21=d; m22=e; m23=f} : transformation) 
          ({ xc=x; yc=y } : point) -> { xc = a*x + b*y + c; yc = d*x + e*y + f }

   /// Applies the identity transformation to the given point.  
   let id = apply identity

   /// Computes the inverse transformation for the input transformation.
   let inverse ({ m11=a; m12=b; m13=c; m21=d; m22=e; m23=f} : transformation) =
       let den = a*e - b*d
       { m11 =  e/den; m12 = -b/den; m13 = (f*b - c*e)/den;
         m21 = -d/den; m22 =  a/den; m23 = (c*d - f*a)/den }
   
   /// Composes a list of transformations into one.
   let compose = 
      let comp : transformation -> transformation -> transformation = 
         fun { m11=a11; m12=a12; m13=a13; m21=a21; m22=a22; m23=a23 }
            { m11=b11; m12=b12; m13=b13; m21=b21; m22=b22; m23=b23 } ->
            { m11=a11*b11 + a12*b21;
               m12=a11*b12 + a12*b22;
               m13=a11*b13 + a12*b23 + a13;
               m21=a21*b11 + a22*b21;
               m22=a21*b12 + a22*b22;
               m23=a21*b13 + a22*b23 + a23 }
      List.fold comp identity

   /// Computes the translation transformation for the given coordinates.
   let translation (x, y) = { m11=1.; m12=0.; m13=x; m21=0.; m22=1.; m23=y }

   /// Computes the rotation transformation for the given point and angle in degrees.
   let rotation p angleDeg = 
       let sin = sinDeg angleDeg
       let cos = cosDeg angleDeg
       let rot = { m11=cos; m12= -sin; m13=0.; m21= sin; m22=cos; m23=0. }
       let tr1 = translation (p.xc, p.yc)
       let tr2 = translation (-p.xc, -p.yc)
       compose [ tr1; rot; tr2 ]

   /// Computes the scaling transformation for the given scaling inputs.
   let scaling (sx, sy) = { m11=sx; m12=0.; m13=0.; m21=0.; m22=sy; m23=0. }

   /// Computes the pivot scaling transformation for the given inputs.
   let pivotScaling p (sx, sy) =
       let tr1 = translation (p.xc, p.yc)
       let tr2 = translation (-p.xc, -p.yc)
       let scl = scaling (sx, sy)
       compose [ tr1; scl; tr2 ]   

   /// Computes the symmetry transformation for the given point.
   let pointSymmetry p = rotation p 180.

   /// Computes the symmetry transformation for the line that passes through 
   /// the two input points.
   let lineSymmetry p1 p2 = 
      let isyaxis = p1.xc = p2.xc
      let isxaxis = p1.yc = p2.yc
      let isaxis = isxaxis || isyaxis
      let slope = 
         if isyaxis then infinity 
         else if isxaxis then 0.
         else (p2.yc - p1.yc)/(p2.xc - p1.xc)
      let angle_w_xaxis = 
         if isyaxis then 90. 
         else if isxaxis then 0.
         else radToDeg (atan slope)
      let yintercept = 
         if isaxis then point.Origin
         else point.Make 0. (p1.yc - slope * p1.xc)
      let tr1 = 
         if isaxis then identity 
         else translation (yintercept.xc, yintercept.yc)
      let tr2 = 
         if isaxis then identity
         else translation (-yintercept.xc, -yintercept.yc)
      let twotheta = 2.*angle_w_xaxis
      let sin = sinDeg twotheta
      let cos = cosDeg twotheta
      let ref = { m11=cos; m12=sin; m13=0.; m21=sin; m22= -cos; m23=0. }
      compose [ tr1; ref; tr2 ]
