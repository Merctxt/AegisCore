 [Voltar para o índice](../README.md)

# Integração com Aplicações

Guias práticos para integrar a AegisCore API em diferentes tipos de aplicações.

---

## Websites / CMS

Valide comentários e formulários em tempo real antes de salvar no banco de dados.

```javascript
// Validação de comentários antes do envio
document.getElementById('comment-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const comment = document.getElementById('comment').value;
    const submitBtn = document.getElementById('submit-btn');
    
    // Desabilitar botão durante análise
    submitBtn.disabled = true;
    submitBtn.textContent = 'Analisando...';
    
    try {
        const response = await fetch('/api/moderate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ text: comment })
        });
        
        const result = await response.json();
        
        if (result.isToxic) {
            showError('Seu comentário contém conteúdo inadequado e não pode ser publicado.');
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
const axios = require('axios');
const io = require('socket.io')(server);

const AEGIS_URL = 'https://localhost:5050';
const AEGIS_KEY = 'aegis_sua_chave';

// Função de moderação
async function moderateMessage(message) {
    const response = await axios.post(`${AEGIS_URL}/api/moderation/analyze`, {
        text: message,
        language: 'pt'
    }, {
        headers: { 'X-Api-Key': AEGIS_KEY }
    });
    
    return response.data;
}

// Middleware de moderação
io.on('connection', (socket) => {
    console.log('Usuário conectado:', socket.id);
    
    socket.on('message', async (data) => {
        try {
            const moderation = await moderateMessage(data.text);
            
            if (moderation.isToxic) {
                // Bloquear mensagem tóxica
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

## Fóruns e Redes Sociais

Implemente diferentes níveis de ação baseado na confiança da detecção.

```javascript
async function moderatePost(postContent) {
    const response = await fetch('/api/moderation/analyze', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Api-Key': 'aegis_sua_chave'
        },
        body: JSON.stringify({
            text: postContent,
            includeAllScores: true
        })
    });
    
    const result = await response.json();
    
    if (!result.isToxic) {
        return { action: 'approve', reason: null };
    }
    
    // Diferentes níveis de ação baseado no score
    const score = result.toxicityScore;
    
    if (score >= 0.9) {
        return { 
            action: 'block', 
            reason: 'Conteúdo altamente tóxico detectado' 
        };
    } else if (score >= 0.7) {
        return { 
            action: 'review', 
            reason: 'Enviado para revisão manual' 
        };
    } else {
        return { 
            action: 'flag', 
            reason: 'Marcado para monitoramento' 
        };
    }
}

// Uso
const moderation = await moderatePost(userPost);

switch (moderation.action) {
    case 'approve':
        await publishPost(userPost);
        break;
    case 'block':
        notifyUser('Seu post não pôde ser publicado: ' + moderation.reason);
        break;
    case 'review':
        await sendToModerationQueue(userPost);
        notifyUser('Seu post está aguardando aprovação.');
        break;
    case 'flag':
        await publishPost(userPost, { flagged: true });
        break;
}
```

---

## APIs e Microserviços

Integre a moderação como middleware em sua API.

### Express.js Middleware

```javascript
const axios = require('axios');

const moderationMiddleware = (options = {}) => {
    const { 
        apiKey,
        apiUrl = 'https://localhost:5050',
        threshold = 0.7,
        textFields = ['content', 'text', 'message']
    } = options;

    return async (req, res, next) => {
        if (req.method !== 'POST' && req.method !== 'PUT') {
            return next();
        }

        // Encontrar texto para moderar
        let textToModerate = null;
        for (const field of textFields) {
            if (req.body[field]) {
                textToModerate = req.body[field];
                break;
            }
        }

        if (!textToModerate) {
            return next();
        }

        try {
            const response = await axios.post(`${apiUrl}/api/moderation/analyze`, {
                text: textToModerate
            }, {
                headers: { 'X-Api-Key': apiKey }
            });

            if (response.data.isToxic && response.data.toxicityScore >= threshold) {
                return res.status(400).json({
                    error: 'Content rejected',
                    message: 'O conteúdo foi identificado como inadequado'
                });
            }

            // Anexar resultado da moderação ao request
            req.moderation = response.data;
            next();

        } catch (error) {
            console.error('Moderation error:', error);
            // Fail-open: permitir em caso de erro
            next();
        }
    };
};

// Uso
app.use('/api/posts', moderationMiddleware({
    apiKey: process.env.AEGIS_API_KEY,
    threshold: 0.6
}));

app.post('/api/posts', (req, res) => {
    // req.moderation contém o resultado da análise
    console.log('Moderation score:', req.moderation?.toxicityScore);
    // ... criar post
});
```

---

## Bots de Discord / Telegram

Modere mensagens automaticamente em servidores.

### Discord.js

```javascript
const { Client, GatewayIntentBits } = require('discord.js');
const axios = require('axios');

const client = new Client({
    intents: [
        GatewayIntentBits.Guilds,
        GatewayIntentBits.GuildMessages,
        GatewayIntentBits.MessageContent
    ]
});

const AEGIS_KEY = 'aegis_sua_chave';

async function moderateText(text) {
    const response = await axios.post('https://localhost:5050/api/moderation/analyze', {
        text: text
    }, {
        headers: { 'X-Api-Key': AEGIS_KEY }
    });
    return response.data;
}

client.on('messageCreate', async (message) => {
    if (message.author.bot) return;

    try {
        const result = await moderateText(message.content);

        if (result.isToxic && result.toxicityScore >= 0.8) {
            await message.delete();
            await message.channel.send({
                content: `${message.author}, sua mensagem foi removida por conteúdo inadequado.`,
                allowedMentions: { users: [message.author.id] }
            });
            
            // Log para moderadores
            console.log(`Mensagem tóxica removida de ${message.author.tag}`);
        }
    } catch (error) {
        console.error('Erro na moderação:', error);
    }
});

client.login(process.env.DISCORD_TOKEN);
```