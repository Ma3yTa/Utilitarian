

let testarr = [|1;2;3;4;5|]
let sliced =  testarr |> Array.splitAt 1

let i = 1
let lastIndex arr = (arr |> Array.length) - 1
let lower = Array.sub testarr 0 (i-1)
let upper = Array.sub testarr (i) (lastIndex testarr)

