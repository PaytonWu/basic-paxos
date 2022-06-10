struct Proposal {
    propose_number: int,
    propose_value: Option<int>,
    from: int,
    to: int,
}

struct Reply {
    ok: bool,
    propose_number: int,
    propose_value: Option<int>,
}