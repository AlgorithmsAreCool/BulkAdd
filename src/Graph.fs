module BulkAdd.Graph

open Global

type Node<'id> = {
    Edges : Node<'id> list
    Id : 'id
}

type private VisitorState<'nodeId, 'state when 'nodeId : comparison> = {
    Traveled : int
    Visited : Set<'nodeId>
    State : 'state
    Inspector : ('nodeId * 'state) -> 'state
}
    with
        member this.Inspect(node) =
            { this with
                Visited = this.Visited.Add(node.Id);
                State = this.Inspector(node.Id, this.State)
                Traveled = this.Traveled + 1
            }

type Inspector<'node, 'state when 'node : comparison> = {
    State : 'state
    Inspector : ('node * 'state) -> 'state
}

type Builder<'node> = {
    GetEdges : 'node -> 'node list
}
let rec private buildNode (builder) (depth) (id) =
    if Debug then
        cprintf Color.Yellow "Building (%d) %A" depth id

    let nodeBuilder = buildNode builder (depth + 1)
    { Id = id; Edges = List.map (nodeBuilder) (builder.GetEdges id) }

let rec private visitNode (visitor : VisitorState<'node, 'state>) (node : Node<'node>)  =
    if Debug then
        cprintf Color.Yellow "Visiting (%d) %A" visitor.Traveled node.Id

    match Set.contains node.Id visitor.Visited with
    | true -> visitor
    | false -> List.fold (visitNode) (visitor.Inspect(node)) (node.Edges)


//Public Functions
let Build(builder, origin) = buildNode builder 0 origin

let Visit(origin, inspector) =
    let visitor = { Traveled = 0; Visited = Set.empty; State = inspector.State; Inspector = inspector.Inspector }
    let result = visitNode visitor origin
    result.State