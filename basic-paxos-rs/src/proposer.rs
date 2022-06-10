struct Proposer<'a> {
    id: int,
    round: int,
    number: int,
    acceptor: &'a[i32]
}

impl<'a> Proposer {
    pub fn new(id: int, acceptor: &'a[i32]) -> Self {
        Proposer{ id, round: 0, number: 0, acceptor};
    }


}