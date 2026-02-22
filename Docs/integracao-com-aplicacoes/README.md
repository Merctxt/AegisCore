 [Voltar para o índice](../README.md)

# Integração com Aplicações

Guias práticos para integrar a AegisCore API em diferentes tipos de aplicações.

---

## Gerenciamento de Token

Antes de integrar, implemente um gerenciador de token:

```javascript
class TokenManager {
  constructor(apiUrl) {
    this.apiUrl = apiUrl;
    this.token = null;
    this.expiresAt = null;
  }
  
  async getToken() {
    // Retorna token existente se ainda válido (com margem de 2 min)
    if (this.token && this.expiresAt > Date.now() + 120000) {
      return this.token;
    }
    
    // Gera novo token
    const response = await fetch(`${this.apiUrl}/api/token/generate`, {
      method: 'POST'
    });
    const data = await response.json();
    
    this.token = data.token;
    this.expiresAt = new Date(data.expiresAt).getTime();
    
    return this.token;
  }
}
```

---

## Websites / CMS

Valide comentários antes de salvar no banco de dados.

```javascript
const tokenManager = new TokenManager('https://api.exemplo.com');

document.getElementById('comment-form').addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const comment = document.getElementById('comment').value;
  const submitBtn = document.getElementById('submit-btn');
  
  submitBtn.disabled = true;
  submitBtn.textContent = 'Analisando...';
  
  try {
    const token = await tokenManager.getToken();
    
    const response = await fetch('https://api.exemplo.com/api/moderation/analyze', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Access-Token': token
      },
      body: JSON.stringify({ text: comment })
    });
    
    const result = await response.json();
    
    if (result.isToxic) {
      showError('Seu comentário contém conteúdo inadequado.');
      return;
    }
    
    // Comentário aprovado - submeter
    await submitComment(comment);
    showSuccess('Comentário publicado!');
    
  } catch (error) {
    showError('Erro ao processar. Tente novamente.');
  } finally {
    submitBtn.disabled = false;
    submitBtn.textContent = 'Enviar';
  }
});
```

---

## Chat em Tempo Real

Modere mensagens antes de transmiti-las para outros usuários.

### Socket.io (Node.js)

```javascript
const io = require('socket.io')(server);

class AegisModerator {
  constructor(apiUrl) {
    this.apiUrl = apiUrl;
    this.token = null;
    this.expiresAt = null;
  }
  
  async getToken() {
    if (this.token && this.expiresAt > Date.now() + 120000) {
      return this.token;
    }
    
    const response = await fetch(`${this.apiUrl}/api/token/generate`, {
      method: 'POST'
    });
    const data = await response.json();
    this.token = data.token;
    this.expiresAt = new Date(data.expiresAt).getTime();
    return this.token;
  }
  
  async moderateMessage(text) {
    const token = await this.getToken();
    
    const response = await fetch(`${this.apiUrl}/api/moderation/analyze`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Access-Token': token
      },
      body: JSON.stringify({ text, language: 'pt' })
    });
    
    return response.json();
  }
}

const moderator = new AegisModerator('https://api.exemplo.com');

io.on('connection', (socket) => {
  console.log('Usuário conectado:', socket.id);
  
  socket.on('message', async (data) => {
    try {
      const moderation = await moderator.moderateMessage(data.text);
      
      if (moderation.isToxic) {
        socket.emit('message_blocked', {
          reason: 'Conteúdo inadequado detectado',
          score: moderation.toxicityScore
        });
        return;
      }
      
      // Mensagem aprovada - transmitir
      io.emit('message', {
        user: data.user,
        text: data.text,
        timestamp: new Date()
      });
      
    } catch (error) {
      console.error('Erro na moderação:', error);
      // Em caso de erro, permitir a mensagem (fail-open)
      io.emit('message', data);
    }
  });
});
```

---

## Backend API (Middleware)

Implemente como middleware no seu backend.

### Express.js

```javascript
const express = require('express');
const app = express();

class AegisModerator {
  // ... (mesma implementação acima)
}

const moderator = new AegisModerator('https://api.exemplo.com');

// Middleware de moderação
async function moderateContent(req, res, next) {
  const textFields = ['content', 'message', 'comment', 'text'];
  
  for (const field of textFields) {
    if (req.body[field]) {
      try {
        const result = await moderator.moderateMessage(req.body[field]);
        
        if (result.isToxic) {
          return res.status(400).json({
            error: 'Conteúdo inadequado',
            field: field,
            score: result.toxicityScore
          });
        }
      } catch (error) {
        console.error('Erro na moderação:', error);
        // Continuar sem moderação em caso de erro
      }
    }
  }
  
  next();
}

// Usar middleware em rotas específicas
app.post('/api/comments', moderateContent, (req, res) => {
  // Salvar comentário
  res.json({ success: true });
});

app.post('/api/posts', moderateContent, (req, res) => {
  // Criar post
  res.json({ success: true });
});
```

---

## Moderação em Lote

Para análise de alto volume, use o endpoint de lote.

```javascript
async function moderateBatch(texts) {
  const token = await tokenManager.getToken();
  
  const response = await fetch('https://api.exemplo.com/api/moderation/analyze/batch', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Access-Token': token
    },
    body: JSON.stringify({
      texts: texts,
      language: 'pt'
    })
  });
  
  const result = await response.json();
  
  // Filtrar textos tóxicos
  const toxic = result.results.filter(r => r.isToxic);
  const safe = result.results.filter(r => !r.isToxic);
  
  console.log(`Total: ${result.totalAnalyzed}, Tóxicos: ${result.toxicCount}`);
  
  return { toxic, safe };
}

// Uso
const comments = ['comentário 1', 'comentário 2', 'comentário 3'];
const { toxic, safe } = await moderateBatch(comments);
```
