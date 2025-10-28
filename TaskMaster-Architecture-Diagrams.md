# TaskMaster Microservices Architecture - Visual Diagrams

## Service Communication Flow

```mermaid
graph TB
    Browser[Browser Client]
    Gateway[Gateway Service<br/>Port 8080<br/>YARP + Correlation IDs]
    UI[UI Service<br/>Port 8081<br/>Razor Pages]
    Task[Task Service<br/>Port 8082<br/>Web API + EF Core]
    Category[Category Service<br/>Port 8083<br/>Web API]
    DB[(SQLite Database<br/>/data/tasks.db<br/>WAL Mode)]
    
    Browser -->|HTTP Requests| Gateway
    Gateway -->|Route /** | UI
    Gateway -->|Route /api/tasks/**| Task
    Gateway -->|Route /api/categories/**| Category
    UI -->|HttpClient via Gateway| Task
    UI -->|HttpClient via Gateway| Category
    Task -->|EF Core| DB
    
    style Gateway fill:#e1f5ff
    style UI fill:#fff4e6
    style Task fill:#e8f5e9
    style Category fill:#f3e5f5
    style DB fill:#fce4ec
```

## Request Flow: View Tasks

```mermaid
sequenceDiagram
    participant B as Browser
    participant G as Gateway
    participant U as UI Service
    participant T as Task Service
    participant D as SQLite DB
    
    B->>G: GET /
    G->>G: Inject X-Correlation-ID
    G->>U: GET / (with correlation ID)
    U->>G: GET /api/tasks?filter=All
    G->>T: GET /api/tasks?filter=All
    T->>D: SELECT * FROM Tasks
    D-->>T: Task rows
    T-->>G: List<TaskDto>
    G-->>U: List<TaskDto>
    U->>U: Render Index.cshtml
    U-->>G: HTML Response
    G-->>B: HTML Response
```

## Request Flow: Create Task

```mermaid
sequenceDiagram
    participant B as Browser
    participant G as Gateway
    participant U as UI Service
    participant T as Task Service
    participant C as Category Service
    participant D as SQLite DB
    
    B->>G: GET /Create
    G->>U: GET /Create
    U->>G: GET /api/categories
    G->>C: GET /api/categories
    C-->>G: Category list
    G-->>U: Category list
    U-->>G: HTML form with categories
    G-->>B: HTML form
    
    B->>G: POST /Create (form data)
    G->>U: POST /Create
    U->>U: Validate ModelState
    U->>G: POST /api/tasks
    G->>T: POST /api/tasks
    T->>D: INSERT INTO Tasks
    D-->>T: New task ID
    T-->>G: TaskDto
    G-->>U: TaskDto
    U->>U: RedirectToPage(Index)
    U-->>G: 302 Redirect
    G-->>B: 302 Redirect to /
```

## Error Handling with Polly

```mermaid
graph TB
    UI[UI Service attempts call]
    Gateway[Gateway Service]
    Task[Task Service]
    
    UI -->|Request| Gateway
    Gateway -->|Forward| Task
    Task -->|FAIL| Gateway
    Gateway -->|Error| UI
    
    UI -->|Polly Retry 1<br/>Wait 2s| Gateway
    Gateway -->|Forward| Task
    Task -->|FAIL| Gateway
    Gateway -->|Error| UI
    
    UI -->|Polly Retry 2<br/>Wait 4s| Gateway
    Gateway -->|Forward| Task
    Task -->|FAIL| Gateway
    Gateway -->|Error| UI
    
    UI -->|Polly Retry 3<br/>Wait 8s| Gateway
    Gateway -->|Forward| Task
    Task -->|FAIL| Gateway
    Gateway -->|Error| UI
    
    UI -->|All retries failed| ErrorPage[Display Error Message<br/>Service Unavailable]
    
    style ErrorPage fill:#ffcdd2
```

## Deployment Architecture

```mermaid
graph TB
    subgraph Fly.io Cloud
        subgraph App1[taskmaster-gateway]
            GW[Gateway Container<br/>Port 8080]
        end
        
        subgraph App2[taskmaster-ui]
            UI[UI Container<br/>Port 8081]
        end
        
        subgraph App3[taskmaster-tasks]
            TS[Task Container<br/>Port 8082]
            VOL[Fly Volume<br/>taskmaster_data]
            TS -.->|Mount /data| VOL
        end
        
        subgraph App4[taskmaster-categories]
            CS[Category Container<br/>Port 8083]
        end
        
        GW -->|Internal DNS| UI
        GW -->|Internal DNS| TS
        GW -->|Internal DNS| CS
    end
    
    Internet[Internet Traffic] -->|HTTPS| GW
    
    style GW fill:#e1f5ff
    style UI fill:#fff4e6
    style TS fill:#e8f5e9
    style CS fill:#f3e5f5
    style VOL fill:#fce4ec
```

## Service Responsibilities Matrix

```mermaid
graph LR
    subgraph Gateway Responsibilities
        GW1[Route Requests]
        GW2[Inject Correlation IDs]
        GW3[Aggregate Health Checks]
    end
    
    subgraph UI Responsibilities
        UI1[Render HTML]
        UI2[Form Validation]
        UI3[Service Orchestration]
        UI4[Error Handling]
    end
    
    subgraph Task Responsibilities
        T1[Task CRUD]
        T2[Business Logic]
        T3[Database Access]
        T4[Data Validation]
    end
    
    subgraph Category Responsibilities
        C1[Category Enum API]
        C2[Validation]
        C3[Reference Data]
    end
    
    style Gateway Responsibilities fill:#e1f5ff
    style UI Responsibilities fill:#fff4e6
    style Task Responsibilities fill:#e8f5e9
    style Category Responsibilities fill:#f3e5f5
```

## Database Access Pattern

```mermaid
graph TB
    subgraph Shared Volume /data
        DB[(tasks.db<br/>WAL Mode Enabled)]
    end
    
    Task[Task Service<br/>TaskDbContext]
    UI[UI Service<br/>No DB Access]
    Category[Category Service<br/>No DB Access]
    Gateway[Gateway Service<br/>No DB Access]
    
    Task -->|EF Core<br/>Full CRUD| DB
    UI -.->|No Direct Access<br/>Uses APIs| Task
    Category -.->|No DB Dependency<br/>Enum-Based| Task
    Gateway -.->|No DB Access<br/>Routes Only| Task
    
    style DB fill:#fce4ec
    style Task fill:#e8f5e9
```
