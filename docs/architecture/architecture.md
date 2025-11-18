graph TB
    Internet[Internet Users]
    CF[Cloudflare Tunnel<br/>cloudflared]
    Nginx[Nginx Reverse Proxy<br/>:80]
    Frontend[React Frontend<br/>Vite + TypeScript]
    Backend[.NET Backend<br/>ASP.NET Core API<br/>:8080]
    DB[(MySQL Database<br/>:3306)]
    RaspPi[Raspberry Pi 5<br/>Docker Host]
    
    Internet -->|HTTPS| CF
    CF -->|HTTP| Nginx
    Nginx -->|Proxy /| Frontend
    Nginx -->|Proxy /api| Backend
    Backend -->|EF Core| DB
    Backend -->|SignalR Hub| Frontend
    
    subgraph DockerNetwork[Docker Network]
        Nginx
        Frontend
        Backend
        DB
        CF
    end
    
    RaspPi -.Contains.-> DockerNetwork
    
    style CF fill:#f96,stroke:#333,stroke-width:2px
    style Nginx fill:#9f6,stroke:#333,stroke-width:2px
    style Frontend fill:#69f,stroke:#333,stroke-width:2px
    style Backend fill:#f93,stroke:#333,stroke-width:2px
    style DB fill:#fc3,stroke:#333,stroke-width:2px