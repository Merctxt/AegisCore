# 🛡️ AegisCore - Documentação Técnica

API REST para moderação automática de conteúdo usando Google Perspective API. Este documento é destinado a desenvolvedores que desejam hospedar sua própria instância.

## 📋 Índice

1. [Instalação e Configuração](./instalacao-configuracao/README.md)
2. [Autenticação](./autenticacao/README.md)
3. [Rate Limiting](./rate-limiting/README.md)
4. [Endpoints](./endpoints/README.md)
5. [Exemplos de Uso](./exemplos-de-uso/README.md)
6. [Códigos de Status](./codigos-de-status/README.md)
7. [Integração com Aplicações](./integracao-com-aplicacoes/README.md)

## 🏗️ Arquitetura

```
AegisCore/
├── AegisCoreApi/          # Backend API (ASP.NET Core)
│   ├── Controllers/       # Endpoints da API
│   ├── Services/          # Lógica de negócio
│   ├── Models/            # Entidades do banco
│   ├── DTOs/              # Data Transfer Objects
│   ├── Data/              # DbContext (PostgreSQL)
│   └── Middleware/        # Autenticação API Key
├── AegisCoreWeb/          # Frontend (ASP.NET MVC)
│   ├── Controllers/       # Controllers MVC
│   ├── Views/             # Razor Views
│   ├── Models/            # ViewModels
│   └── Services/          # Serviços HTTP
└── Docs/                  # Documentação
```

## 🛠️ Instalação e Configuração

### Pré-requisitos
- .NET 10 SDK
- PostgreSQL 14+
- Google Perspective API Key

### 1. Clone o repositório
```bash
git clone https://github.com/Merctxt/AegisCore.git
cd AegisCore
```

### 2. Configure as variáveis de ambiente
```bash
cp .env.example .env
```

Edite o arquivo `.env`:
```env
# Database (PostgreSQL)
DATABASE_URL=postgresql://user:password@localhost:5432/aegiscore

# JWT Authentication
JWT_SECRET=sua-chave-secreta-com-pelo-menos-32-caracteres!

# Google Perspective API
PERSPECTIVE_API_KEY=sua_chave_aqui
```

### 3. Execute as migrations
```bash
cd AegisCoreApi
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Inicie a API
```bash
dotnet run --project AegisCoreApi
```

A API estará disponível em `https://localhost:5050`

### 5. Inicie o Frontend (opcional)
```bash
dotnet run --project AegisCoreWeb
```

## 🔐 Autenticação

### JWT Token (para Dashboard)
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@email.com",
  "password": "suasenha"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-22T12:00:00Z",
  "user": {
    "id": "guid",
    "name": "Nome",
    "email": "email@email.com",
    "plan": "Free"
  }
}
```

### API Key (para Moderation Endpoints)
```http
POST /api/moderation/analyze
X-Api-Key: aegis_sua_chave_aqui
Content-Type: application/json

{
  "text": "Texto para analisar"
}
```

## ⚡ Rate Limiting

| Plano      | Requisições/dia |
|------------|-----------------|
| Free       | 100             |
| Starter    | 1,000           |
| Pro        | 10,000          |
| Enterprise | Ilimitado       |

## 📡 Endpoints Principais

### Health Check
```http
GET /health
```

### Análise de Texto
```http
POST /api/moderation/analyze
X-Api-Key: sua_chave

{
  "text": "Texto para analisar",
  "language": "pt",
  "includeAllScores": false
}
```

**Resposta:**
```json
{
  "isToxic": false,
  "toxicityScore": 0.12,
  "allScores": null,
  "analyzedText": "Texto para analisar",
  "timestamp": "2025-01-15T10:30:00Z"
}
```

### Análise em Lote
```http
POST /api/moderation/analyze/batch
X-Api-Key: sua_chave

{
  "texts": ["texto1", "texto2", "texto3"],
  "language": "pt"
}
```

## 🔗 Webhooks

Configure webhooks para receber notificações quando conteúdo tóxico for detectado:

```http
POST /api/webhooks
Authorization: Bearer seu_jwt_token

{
  "name": "Alertas Toxicidade",
  "url": "https://seu-servidor.com/webhook",
  "secret": "chave_para_validacao",
  "events": 1
}
```

### Eventos Disponíveis
- `1` - Conteúdo Tóxico
- `2` - Alta Toxicidade (>90%)
- `4` - Rate Limit Atingido
- `7` - Todos os Eventos

### Payload do Webhook
```json
{
  "event": "ToxicContent",
  "timestamp": "2025-01-15T10:30:00Z",
  "data": {
    "text": "texto analisado",
    "toxicityScore": 0.85,
    "analyzedAt": "2025-01-15T10:30:00Z"
  }
}
```

### Validação de Assinatura
Se você configurou um `secret`, valide a assinatura:
```
X-Aegis-Signature: sha256=hash_hmac_do_payload
```

## 🐳 Docker (Em breve)

```dockerfile
# Dockerfile disponível em breve
docker-compose up -d
```

## 📄 Licença

MIT License - Veja [LICENSE](../LICENSE) para mais detalhes.
  }
}
```

### 3. Moderação Básica
```http
POST /moderate
```

**Body:**
```json
{
  "text": "Texto para analisar",
  "thresholds": {
    "toxicity": 0.7,
    "severeToxicity": 0.8
  },
  "languages": ["pt", "en"]
}
```

**Resposta:**
```json
{
  "success": true,
  "requestId": "uuid-4-request",
  "data": {
    "text": "Texto para analisar",
    "isToxic": false,
    "action": "allow",
    "reason": "Content is safe",
    "confidence": 15,
    "violations": [],
    "timestamp": "2025-08-27T10:30:00.000Z"
  }
}
```

### 4. Análise Detalhada
```http
POST /analyze
```

**Body:**
```json
{
  "text": "Texto para análise detalhada",
  "thresholds": {
    "toxicity": 0.7,
    "insult": 0.6
  },
  "languages": ["pt"],
  "includeScores": true
}
```

**Resposta:**
```json
{
  "success": true,
  "requestId": "uuid-4-request",
  "data": {
    "text": "Texto para análise detalhada",
    "analysis": {
      "isToxic": false,
      "confidence": 12,
      "maxScore": 0.12,
      "violations": [],
      "reason": "Content is safe"
    },
    "recommendation": {
      "action": "allow",
      "severity": "low"
    },
    "scores": {
      "toxicity": 0.12,
      "severeToxicity": 0.05,
      "identityAttack": 0.08,
      "insult": 0.10,
      "profanity": 0.03,
      "threat": 0.02
    },
    "metadata": {
      "textLength": 32,
      "timestamp": "2025-08-27T10:30:00.000Z",
      "thresholds": {...}
    }
  }
}
```

### 5. Análise em Lote (🔒 Requer API Key)
```http
POST /batch
X-API-Key: sua-api-key
```

**Body:**
```json
{
  "texts": [
    "Primeiro texto",
    "Segundo texto",
    "Terceiro texto"
  ],
  "thresholds": {
    "toxicity": 0.7
  },
  "maxConcurrent": 3
}
```

**Resposta:**
```json
{
  "success": true,
  "requestId": "uuid-4-request",
  "data": {
    "results": [
      {
        "index": 0,
        "text": "Primeiro texto",
        "isToxic": false,
        "confidence": 10,
        "violations": [],
        "action": "allow"
      }
    ],
    "summary": {
      "total": 3,
      "toxic": 0,
      "safe": 3,
      "errors": 0
    },
    "timestamp": "2025-08-27T10:30:00.000Z"
  }
}
```

### 6. Estatísticas (🔒 Requer API Key)
```http
GET /stats
X-API-Key: sua-api-key
```

**Resposta:**
```json
{
  "success": true,
  "data": {
    "api": {
      "version": "1.0.0",
      "uptime": 3600,
      "memoryUsage": {...},
      "platform": "win32"
    },
    "configuration": {
      "toxicityThreshold": 0.7,
      "rateLimitPerMinute": 100,
      "maxTextLength": 3000
    }
  }
}
```

## 💡 Exemplos de Uso

### JavaScript/Node.js
```javascript
const axios = require('axios');

// Moderação básica
async function moderateText(text) {
  try {
    const response = await axios.post('http://localhost:3000/moderate', {
      text: text,
      thresholds: {
        toxicity: 0.6,
        insult: 0.7
      }
    });
    
    const { isToxic, action, confidence } = response.data.data;
    
    if (isToxic) {
      console.log(`Conteúdo bloqueado! Confiança: ${confidence}%`);
    } else {
      console.log('Conteúdo aprovado!');
    }
    
    return response.data;
  } catch (error) {
    console.error('Erro na moderação:', error.message);
  }
}

// Uso
moderateText("Este é um texto para testar");
```

### Python
```python
import requests

def moderate_text(text):
    url = "http://localhost:3000/moderate"
    data = {
        "text": text,
        "thresholds": {
            "toxicity": 0.7
        }
    }
    
    response = requests.post(url, json=data)
    result = response.json()
    
    if result["success"]:
        analysis = result["data"]
        if analysis["isToxic"]:
            print(f"Conteúdo tóxico detectado! Ação: {analysis['action']}")
        else:
            print("Conteúdo seguro")
    
    return result

# Uso
moderate_text("Texto para análise")
```

### cURL
```bash
# Moderação básica
curl -X POST http://localhost:3000/moderate \
  -H "Content-Type: application/json" \
  -d '{
    "text": "Texto para analisar",
    "thresholds": {
      "toxicity": 0.7
    }
  }'

# Análise em lote (com API key)
curl -X POST http://localhost:3000/batch \
  -H "Content-Type: application/json" \
  -H "X-API-Key: sua-api-key" \
  -d '{
    "texts": ["Texto 1", "Texto 2"],
    "thresholds": {"toxicity": 0.6}
  }'
```

### PHP
```php
<?php
function moderateText($text) {
    $url = 'http://localhost:3000/moderate';
    $data = [
        'text' => $text,
        'thresholds' => [
            'toxicity' => 0.7
        ]
    ];
    
    $options = [
        'http' => [
            'header' => "Content-Type: application/json\r\n",
            'method' => 'POST',
            'content' => json_encode($data)
        ]
    ];
    
    $context = stream_context_create($options);
    $result = file_get_contents($url, false, $context);
    
    return json_decode($result, true);
}

// Uso
$result = moderateText("Texto para verificar");
if ($result['data']['isToxic']) {
    echo "Conteúdo bloqueado!";
} else {
    echo "Conteúdo aprovado!";
}
?>
```

## 🌐 Integração com Aplicações

### Websites/CMS
```javascript
// Validação de comentários em tempo real
document.getElementById('comment-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const comment = document.getElementById('comment').value;
    
    try {
        const response = await fetch('/api/moderate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text: comment })
        });
        
        const result = await response.json();
        
        if (result.data.isToxic) {
            alert('Seu comentário contém conteúdo inadequado e não pode ser publicado.');
            return;
        }
        
        // Submeter comentário
        submitComment(comment);
    } catch (error) {
        console.error('Erro na moderação:', error);
    }
});
```

### Chat Applications
```javascript
// Middleware para chat em tempo real
const moderateMessage = async (message) => {
    const response = await axios.post('http://localhost:3000/moderate', {
        text: message,
        thresholds: { toxicity: 0.6 }
    });
    
    return response.data.data;
};

// Socket.io exemplo
io.on('connection', (socket) => {
    socket.on('message', async (data) => {
        const moderation = await moderateMessage(data.text);
        
        if (moderation.isToxic) {
            socket.emit('message_blocked', {
                reason: moderation.reason,
                confidence: moderation.confidence
            });
        } else {
            io.emit('message', data);
        }
    });
});
```

### Fóruns/Redes Sociais
```javascript
// Moderação de posts
const moderatePost = async (postContent) => {
    const response = await fetch('http://localhost:3000/analyze', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            text: postContent,
            includeScores: true,
            thresholds: {
                toxicity: 0.7,
                insult: 0.6,
                threat: 0.5
            }
        })
    });
    
    const result = await response.json();
    const analysis = result.data.analysis;
    
    if (analysis.isToxic) {
        if (analysis.confidence > 90) {
            return 'block'; // Bloquear imediatamente
        } else if (analysis.confidence > 70) {
            return 'review'; // Enviar para revisão manual
        } else {
            return 'flag'; // Apenas marcar para monitoramento
        }
    }
    
    return 'approve';
};
```

## 📊 Códigos de Status HTTP

| Código | Significado | Descrição |
|--------|-------------|-----------|
| 200 | OK | Requisição processada com sucesso |
| 400 | Bad Request | Dados inválidos na requisição |
| 401 | Unauthorized | API key inválida ou ausente |
| 404 | Not Found | Endpoint não encontrado |
| 429 | Too Many Requests | Rate limit excedido |
| 500 | Internal Server Error | Erro interno do servidor |

## ⚙️ Configurações Avançadas

### Thresholds Personalizados
```json
{
  "text": "Texto para analisar",
  "thresholds": {
    "toxicity": 0.8,        // Toxicidade geral
    "severeToxicity": 0.9,  // Toxicidade severa
    "identityAttack": 0.7,  // Ataques de identidade
    "insult": 0.6,          // Insultos
    "profanity": 0.5,       // Palavrões
    "threat": 0.8           // Ameaças
  }
}
```

### Rate Limiting Personalizado
Edite no código da API:
```javascript
const rateLimiter = new RateLimiterMemory({
    points: 200,      // Requests por período
    duration: 60,     // Período em segundos
});
```

## 🔒 Segurança

### Boas Práticas
1. **Mude a API key padrão** no `.env`
2. **Use HTTPS** em produção
3. **Implemente logs de auditoria**
4. **Configure firewall** para limitar acesso
5. **Monitore uso da API** regularmente

### Exemplo de Proxy Reverso (Nginx)
```nginx
server {
    listen 80;
    server_name sua-api.exemplo.com;
    
    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        
        # Rate limiting adicional
        limit_req zone=api_limit burst=20 nodelay;
    }
}
```

## 🚀 Deploy em Produção

### PM2 (Recomendado)
```bash
npm install -g pm2

# Iniciar
pm2 start ModApi/ModBotApi.js --name "modbot-api"

# Configurar auto-restart
pm2 startup
pm2 save
```

### Docker
```dockerfile
FROM node:18-alpine

WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production

COPY . .

EXPOSE 3000
CMD ["node", "ModApi/ModBotApi.js"]
```

### Variáveis de Ambiente para Produção
```env
NODE_ENV=production
API_PORT=3000
API_SECRET_KEY=chave-super-secreta-production
PERSPECTIVE_API_KEY=sua_chave_production
```

## 📈 Monitoramento

### Health Check
Configure monitoramento automático:
```bash
# Verificar se a API está funcionando
curl -f http://localhost:3000/health || exit 1
```

### Logs
Os logs incluem:
- Todas as requisições com request ID
- Erros da Perspective API
- Rate limiting ativado
- Estatísticas de uso

## 🆘 Suporte

Para problemas específicos da API:
1. Verifique os logs do console
2. Teste com `curl` ou Postman
3. Confirme que a Perspective API está funcionando
4. Verifique rate limits

---

**Versão:** 1.0.0  
**Última atualização:** Agosto 2025
