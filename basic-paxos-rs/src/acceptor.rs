use std::net::TcpListener;

struct Acceptor {
    listener: TcpListener,
    id: int,
    minProposer: int,
    acceptedNumber: int,
    acceptedValue: int,
}

impl Acceptor {
    pub fn new(listener: TcpListener, id: int) -> Self {
        Acceptor{listener, id, minProposer: 0, acceptedNumber: 0, acceptedValue: 0}
    }
}