use std::io::prelude::*;
use std::net::TcpListener;
use std::net::TcpStream;

fn handle_connection(mut stream: TcpStream) {
    println!("start handling connection");

    let mut buffer = [0; 4];
    let mut n = 0;
    while n < 12 {
        let m = stream.read(&mut buffer)?;
        print!("{}", String::from_utf8_lossy(&buffer));
        stream.write(&buffer[..m])?;
        n += m;
    }
    println!("");

    stream.flush()?;
    println!("end handle connection");
}

struct PaxosServer {
    tcpStream: TcpStream,
    address: std::net::IpAddr,
    port: u16,
}

impl BasicPaxosServer {
    fn handle_connection(&mut self) {
        println!("start handling connection");

        let mut buffer = [0; 4];
        let mut n = 0;

        while n < 12 {
            let m = self.tcpStream.read(&mut buffer)?;
            n += m;
        }
        println!("");

        self.tcpStream.flush()?;
        println!("end handle connection");
    }
}