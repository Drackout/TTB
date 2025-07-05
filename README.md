
# Sistemas de Redes para Jogos

### Micael Teixeira - 22208717

## PROJETO: Jogo online baseado em turnos, cliente/servidor, sem login/matchmaking

  

## Resumo:

### O Jogo:
O jogo criado, é um jogo por turnos em que se tem 2 jogadores ligados a um servidor, (para já na mesma máquina com localhost) e que estes têm 5 rondas para derrotar todos os inimigos (os inimigos não têm turno e não executam ações). 

Um dos jogadores é o anfitrião enquanto que os restantes são clientes que se vão conectar ao mesmo. Cada jogador tem 100 de vida e 100 de energia, estes valores vão sendo modificados com o decorrer do jogo, a energia quando fazemos ações e a vida quando somos atacados.

Durante o turno de um jogador, este tem a possibilidade de andar (cima, baixo, esquerda e direita) utilizando 20 de energia por passo, o jogador também pode atacar uma unidade utilizando 30 de energia, para efetuar o ataque o jogador atual seleciona uma unidade no jogo (inimigo ou jogador), e este será o alvo do ataque. Após ter completado o seu turno, o jogador pode terminar o turno, fazendo com que seja o turno do próximo jogador.
  

## Descrição técnica:


Para este projeto foram utilizadas as __Raw Sockets__ com o protocolo __TCP__ para efetuar as conexões do servidor e dos clientes. Para a transmissão de pacotes de informação foi utilizado o __RPC__.

A utilização das __Raw Sockets__, sendo de baixo nível, foram pensadas para que se pudesse criar um sistema que garantisse o controlo da comunicação do cliente e do servidor conforme as configurações necessárias. 

Foi escolhido fazer o projeto com o protocolo __TCP__, porque este necessita das conexões bem estabelecidas antes de poder existir troca de informação, garantindo assim que apenas o número máximo de jogadores se pode conectar e que apenas estes vão enviar e receber informações, fazendo assim com que exista um género de ponte entre o servidor e os clientes. 
O protocolo TCP também envia os dados garantidamente de um lado para o outro, pois este protocolo faz a validação dos pacotes de informação enviados, vendo a ocorrência de erros e se existir voltam a pedir esses mesmos pacotes.

Uma possível __desvantagem__ do protocolo TCP poderia ser um aumento na latência se existir retransmissão por causa de alguma perda ou corrupção de alguns pacotes, fazendo com que o protocolo tivesse de os pedir novamente. Neste caso sendo um jogo por turnos a latência não tem um grande impacto por não ser preciso tempo de reação ou disparar um projétil contra alguém em movimento.

 Foi descartada a ideia de usar o protocolo UDP, por vários motivos, sendo estes:
- A sua ordem "aleatória" de chegada da informação não ajudava com o que se queria desenvolver, possivelmente dando origem a múltiplos futuros erros e uma grande perda de tempo para os corrigir.
- Tendo uma transmissão mais rápida que o protocolo TCP, iria influenciar na latência, mas esta não é algo que tenha um grande impacto neste género de jogo.
- Seria preciso fazer variadas verificações, por este não necessitar de uma conexão, apenas envia pacotes de informação para um destino.

O __RPC__ (Remote Procedure Call) foi o modelo de comunicação utilizado para os pacotes de informação que são enviados, neste caso enviando a função que queremos executar do outro lado. Por exemplo, no caso do jogador querer andar para um lado, este envia o nome da função a ser executada para o servidor, entretanto o servidor irá executar a função e retornará uma resposta se pode ou não efetuar o movimento. 
  

O __Multithread__ foi utilizado para quando um cliente se conecta ao servidor, este faz com que seja criado um thread próprio para esse jogador. Isto faz com que os seus inputs e dados, sejam tratados independentemente, facilitando assim as várias conexões ao servidor e permitindo que cada client mexa no seu jogo sem impactar os outros ou com que a aplicação bloqueie à espera que seja o seu turno. E assim fazendo com multiplos jogadores tenham acesso à aplicação ao mesmo tempo, por exemplo (algo que não está no jogo) seria, o permitir que enquanto um está a fazer o seu turno, os outros possam estar a ver o mapa ou as habilidades de outro jogador.

  
Para enviar as mensagens utilizando o modelo RPC, tanto no cliente como no servidor, foram criadas 2 classes para o cliente e 2 classes para o servidor, cada uma delas server para enviar e receber os pacotes de informação, 
Estas classes são espelhadas tanto no cliente como no servidor.

No __Servidor__ este possui as seguintes classes:
- SendMessage, que serve para enviar as mensagens para o cliente.
```
public  class  SendMessage
{
	public  int  playerid { get; set; }
	public  string  action { get; set; }
	public  string  message { get; set; }
	public  string[] extra { get; set; }
}
```
- ClientCommand, que serve para receber as mensagens do cliente.
```

public  class  ClientCommand
{
	public  int  playerid { get; set; }
	public  string  action { get; set; }
	public  string  target { get; set; }
	public  string[] extra { get; set; }
}
```
E no __Cliente__ este possui as seguintes classes:
- SendMessage, que serve para receber as mensagens do servidor.
```
[Serializable]
public  class  ServerMessage
{
	public  int  playerid;
	public  string  action;
	public  string  message;
	public  string[] extra;
}
```
- PlayerCommandSend, que serve para enviar as mensagens para o servidor.
```
[Serializable] //for json utility
public  class  PlayerCommandSend
{
	public  int  playerid;
	public  string  action;
	public  string  target;
	public  string[] extra;
}
```
Contudo no cliente, este estando a ser utilizado o Unity, é necessário incluir o __[Serializable]__ para que este seja possivel converter em bytes e funcionar com o JsonUtility, fazendo assim com que funcione da mesma maneira que o servidor utilizando o JsonSerializer da biblioteca Json.

O receber uma mensagem, seja no cliente ou no servidor, segue os seguintes passos:
- É feita uma primeira verificação dos 4 bytes recebidos numa mensagem para que não haja uma possivel manipulação dos bytes enviados e para saber qual o tamanho da mensagem a receber. Primeiro é criado um array de 4 bytes que recebe os primeiros 4 bytes de uma mensagem. 
- Segundo, é guardado o tamanho da verdadeira mensagem, convertendo-a para um valor inteiro. 
- Com o  tamanho da mensagem que foi guardado, este usa esse valor para criar um novo array the bytes, que depois irá receber a mensagem na totalidade no formato Json.
- Após termos a mensagem completa, esta é convertida para uma string com UTF8 para ter acesso a todos os characteres incluindo os especiais.
- Por fim, faz-se a deserialização da mensagem em Json, e utilizando a classe que foi criada para o  RPC, esta irá receber as informações sendo no cliente ou servidor, podendo aceder as informações em sepatado por cada variável da classe.  

Ambos o servidor e os seus clientes, possuem a mesma lógica, sendo esta o estarem sempre disponiveis para receber qualquer mensagem, sendo possivel quando uma mensagem é trocada (entre um cliente e servidor e vice-versa), o que receber irá usar as informações e internamente fazer o que é pedido na mensagem. 

Foi criada uma classe __Entity.cs__ no servidor, para que cada jogador e inimigo tenha a sua vida, energia, movimento e o dano. Esta classe foi criada para facilitar a gestão de cada entidade dentro do jogo, permitindo saber os seus dados e fazer a gestão dos mesmos conforme o jogo vai decorrendo e os clientes vão enviando ações.
  

## O funcionamento do servidor começa por:

__1) Instanciar e inicializar variáveis globais__, desde uma lista de clients, outra lista com os IDs dos jogadores, turno, rondas, número máximo de jogadores e também cria uma lista com as instancias dos jogadores, estas que serão modificadas conforme o decorrer do jogo com ações recebidas dos clientes.

__2) Criação do endpoint e de um listener__, começa-se por criar uma socket
```
Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
```

Esta socket usa o IPv4 e é do tipo Stream e com o protocolo TCP, fazendo com que se obtenha um fluxo contínuo de dados com entrega garantida e ordenada dos dados. Logo de seguida é criado o EndPoint, onde os clientes se vão conectar, tendo este um port (2100) e um IP em que estamos a utilizar o localhost, este permite qualquer conexão local.

```
IPEndPoint(IPAddress.Any, 2100);
listener.Bind(localEndPoint);
listener.Listen(maxPlayers);
```
Fazendo o .Bind() para ativar a socket comforme o IP e o port fornecido e depois o .Listen(maxPlayers), que serve para estar à escuta de ligações dos clientes até um número dados, sendo este o maxPlayers, faz com que apenas o número máximo de jogadores seja permitido conectar-se, e fazendo com que os restantes não consigam estabelecer uma conexão ao servidor.

  

__3) Ouvir e aceitar conexões__, após o servidor estar à espera de conexões de clients, é efetuado um ciclo que fará com que cada conexão recebida, seja aceite e de seguida adicionada a uma lista de sockets chamada clients, que será utilizada para chamar o respetivo jogador e enviar informação e também atribuindo o número de chegada como o ID do jogador. No final é criado uma Thread cada nova conexão, fazendo com que se trate de todos os jogadores em simultaneo.

```
listener.Accept();
clients.Add(client);
assignedId = clients.Count;
playerIds.Add(assignedId);
Thread clientThread = new Thread(() => HandleClient(client, assignedId));
clientThread.Start();
Broadcast($"Player {assignedId} connected, updatePlayerList ");

```

__4) Tratamento individual de cada cliente__, é efetuado com o iniciar a Thread criada anteriormente, e que começa por enviar uma mensagem ao cliente que acabou de se juntar com o ID par a sessão de jogo.

``` 
SAttachIDtoNewPlayer(client, playerId); 

static  void  SAttachIDtoNewPlayer(Socket  client, int  newPlayerID)
{
	Console.WriteLine($"Give ID to player:{newPlayerID}");
	Send(client, action: "getid", playerId: newPlayerID);
}
```
Logo de seguida envia uma funcionalidade e uma mensagem para todos os clientes conectados, fazendo com que estes atualizarem a lista de jogadores e informando a conexão de um novo jogador.

  

__4.1) Receber e processar as informações dos clientes__, o servidor começa por estar num ciclo que corre enquanto o client estiver conectado e enquanto o número de rondas for inferior ao valor estipulado.

Após recebermos uma mensagem Json e inserido os seus valores num objecto da devida class utilizando a deserialização, a mensagem fica partida em, "playerID" referente a quem enviou a mensagem, "action" para sabermos o tipo de ação do jogador, "target" se ataque quem atacar ou se andar, para que direção e por fim um array de strings "extra", permitindo que sejam enviadas outras informações se visto que necessário.  
```
command  =  JsonSerializer.Deserialize<ClientCommand>(msg);

command.playerid;
command.action;
command.target;
command.extra;
```
__4.2) Enviar informações para os clients__, existem duas maneiras de enviar informações aos clientes, uma que envia apenas ao próprio client __Send()__ em que utilizando o modelo RPC, usamos uma classe, para guardar os dados que vêm como parametros, e logo de seguida converter em Json, e depois de estar em Json, converte-se o mesmo em bytes. Como a secção de receber uma mensagem está à espera primeiro do tamanho da mensagem e só depois a mensagem em si, é enviado o tamanho da mensagem e a mensagem logo de seguida.
- Send():
```
static  void  Send(Socket  client, string  action  =  "", string  message  =  "", int  playerId  =  0, string[] extra  =  null)
{
	if (extra  ==  null)
		extra  =  new  string[0];
		
	//Object to Send Message
	SendMessage  msg  =  new  SendMessage()
	{
		playerid  =  playerId,
		action  =  action,
		message  =  message,
		extra  =  extra
	};
	
	// Convert the Object into Json
	var  setting  =  new  JsonSerializerOptions { PropertyNamingPolicy  =  null }; 
	string  json  =  JsonSerializer.Serialize(msg, setting);
	byte[] data  =  Encoding.UTF8.GetBytes(json);
	byte[] dataLen  =  BitConverter.GetBytes((UInt32)data.Length);
	
	// Send first the Length and then the message
	try
	{
		client.Send(dataLen);
		client.Send(data);
	}
	catch (Exception  e)
	{
		Console.WriteLine("Error sending message to client: "  +  e);
	}
}
```
Ao fazer a conversão do Objecto para Json é utilizado ```var setting = new JsonSerializerOptions { PropertyNamingPolicy = null }; ``` para que os campos que são passados não vão Camel Case. 

Isto foi efetuado por existirem descrepansias do Json do c# e o Json do Unity, tais como no cliente (Unity) para converter em Json usa-se ```JsonUtility.ToJson(msg);``` enquanto que no C# o ```JsonSerializer.Serialize(msg)```, este que  faz com que os campos do Json estejam em Camel Case contráriamente ao .Tojson().

Também é possivel enviar uma mensagem para todos os clientes, em que se baseia na função Send(), só que em vez de receber um socket, irá percorrer a lista de clients que é uma lista de sockets, e para cada um deles irá enviar a mensagem.

Em baixo foram criados dois auxiliares para enviar mensagens simples sem ações para os clientes para que sejam representadas nas respetivas caixas de texto.
```
static  void  BroadcastMessage(string  msg)
{
	Broadcast(message: msg);
}

static  void  SendMessage(Socket  client, string  msg)
{
	Send(client, message: msg);
}
```
  

__4.3) Processamento das mensagens recebidas__, após recebermos a mensagem, como dito anteriormente, esta será retirada do Json e posta num objecto e que logo de seguida será utilizada a parte desse objecto que possui a ação. 
Tendo a ação do cliente que se pretende executar como mensagem no servidor, é feito um Switch com todas as ações possiveis. 
```
switch (command.action)
{
	case  "launch":
		SGameStart(client);
		continue;
  
	case  "move":
		SMove(client, playerId, CliTrgt);
		continue;
  
	case  "attack":
		SAttack(CliTrgt, playerId, CliID, client);
		continue;
  
	case  "endturn":
		SEndTurn();
		continue;
	
	default:
		SendMessage(client, "Unknown action");
		continue;
}
```
Cada uma das ações corresponde ao que o jogador pode fazer do seu lado, seja andar com o seu personagem, atacar, começar o jogo ou mesmo terminar o seu turno.

Todas as ações que foram pedidas pelo cliente e efetuadas, será sempre enviada uma mensagem para todos os jogadores a dizer o que se passou no turno, dizendo qual foi o jogador e a sua ação. 

As acções que um cliente pode pedir são, 
- __"launch"__ que chama a função SGameStart(), esta que recebe o socket de quem enviou a mensagem, no caso de não existirem jogadores suficientes para o informar. Esta função irá verificar quantos jogadores estão conectados ao servidor, irá colocar a variável do inicio do jogo a "True" e enviar uma ação a todos os clientes para que este mostre o jogo em sí.

```
static  void  SGameStart(Socket  client)
{
	Console.WriteLine($"Start Game");
	if (clients.Count  ==  maxPlayers)
	{
		gameStarted  =  true;
		Broadcast(action: "gamestart", playerid: playerIds[currentTurn]);
	}
	else
		Send(client, message: $"Need more players - {clients.Count}/{maxPlayers}");
}
```
- __"move"__ que chama a função SMove(), começa por preparar as informações recebidas do cliente, sendo estas, qual jogador está a ser utilizado, a sua energia e qual o seu movimento, estes dados vindo todos da sua class. A energia necessária para efetuar uma ação de movimento é de 20, se for possível o jogo efetua o movimento e retira do seu objecto, se não for possivel, o jogador que tentou efetuar a ação é notificado que não tem energia suficiente.
```
static  void  SMove(Socket  client, int  playerId, string  target)
{
	int  IDPrefab  =  playerId  -  1;
	if (gameStarted)
	{
		int  energy  =  playersPrefabInfo[IDPrefab].getEnergy();
		int  movement  =  playersPrefabInfo[IDPrefab].getMovement();
  
		string[] extraSend;
		if (energy  >=  20)
		{
			playersPrefabInfo[IDPrefab].takeEnergy(20);
			string  energyLeft  =  playersPrefabInfo[IDPrefab].getEnergy().ToString();
			extraSend  =  new  string[] { movement.ToString(), energyLeft};
			Broadcast(target, "canmove", playerId, extraSend);
			BradcastMessage($"Player{playerId} walked {movement} units {target}, has{energyLeft} energy left!");
		}
		else
		{
			Send(client, message: $"Not enough Energy! Player energy: {energy}");
		}
	}
}
```

-  __"attack"__ que chama a função SAttack(), e parecida com a função de movimento, começa por preparar as informações necessárias e também verifica a energia do jogador, neste caso necessita de 30, não tendo energia  irá notificar o jogador, no caso de ter energia, faz-se a distinção se está a ser atacado o inimigo ou um outro jogador, e também é informado se apenas fez dano ou se deu o golpe final. Após termos todas as informações, é efetuada uma mensagem para todos os jogadores com o que aconteceu.
```
static  void  SAttack(string  target, int  playerId, int  attacker,Socket  client)
{
	string[] targetInfo  =  target.ToLower().Split(" ");
	string  unit  =  targetInfo[0];
	int  unitID  =  Convert.ToInt16(targetInfo[1]);
	int  attackDamage  =  playersPrefabInfo[playerId  -  1].getDamage();
	int  hpRemaining  =  0;
	string[] exHp  =  null;
	string  actionSend;
	int  energy  =  playersPrefabInfo[playerId  -  1].getEnergy();
	if (energy  >=  30)
	{
		if (unit  ==  "enemy")
		{
			enemyPrefabInfo[unitID].loseHP(attackDamage);
			hpRemaining  =  enemyPrefabInfo[unitID].getHP();
			exHp  =  new  string[] { hpRemaining.ToString(), (energy-30).ToString(), attacker.ToString()};
		}
		else  if (unit  ==  "player")
		{
			playersPrefabInfo[unitID  -  1].loseHP(attackDamage);
			hpRemaining  =  playersPrefabInfo[unitID  -  1].getHP();
			exHp  =  new  string[] { hpRemaining.ToString(), (energy-30).ToString(), attacker.ToString()};	
		}

		if (hpRemaining  >  0)
		actionSend  =  "attack";
		else
		actionSend  =  "killed";
  
		playersPrefabInfo[playerId  -  1].takeEnergy(30);
		Broadcast(unit, actionSend, unitID, exHp);
		BroadcastMessage($"Player{playerId}  {actionSend}  {unit}{unitID}, dealing {attackDamage} damage, {hpRemaining} HP left, has {energy-30} energy left!");
	}
	else
	{
		Send(client, message: $"Not enough Energy! Player energy: {energy}");
	}
}
```
 
- __"endturn"__ que irá chamar a função SEndTurn(), que irá passar o turno dos jogadores um a um, ver quando já terminou a ronda e no final dizer a todos os jogadores que o jogador atual terminou o seu turno e informar quem é o próximo.
```
static  void  SEndTurn()
{
	BroadcastMessage($"Player{currentTurn  +  1} - finished his turn");
	playersPrefabInfo[currentTurn].refilEnergy();
  
	string  currentEnergy  =  playersPrefabInfo[currentTurn].getEnergy().ToString();  
	currentTurn  = (currentTurn  +  1) %  playerIds.Count;
  
	if (currentTurn  ==  0)
	{
		rounds++;
		BroadcastMessage($"--- Round {rounds} ---");
	}
  
	int  nextPlayerId  =  playerIds[currentTurn];
	Broadcast(currentEnergy, "nextturn", nextPlayerId);
}
``` 
No caso de ser inserida uma ação que o servidor não reconheça, também é enviada uma mensagem a informar o acontecimento.
E também no caso de um jogador tentar jogar no turno de outro jogador, o servidor irá verificar o que está a tentar ser feito e mostrar que não é o turno desse jogador
```
if (playerIds[currentTurn] !=  playerId)
{
	SendMessage(client, "Not your turn.");
	continue;
}
```


## Os Clientes:
Todos os clientes, são tratados da mesma forma, seja este o anfitrião ou apenas um cliente, 

Para ser possível lançar o jogo, este tem de ser o Host, ou seja, o primeiro jogador a conectar-se ao servidor, também é necessário ter todos os jogadores dentro do servidor, caso não aconteça o servidor alerta ao host que ainda faltam jogadores.

Apenas com o iniciar do jogo, o servidor agora começa a poder processar as ações dos clients, recebendo o movimento, os ataques e quando estes querem terminar o seu turno. No exemplo do jogador querer andar, irá enviar ao servidor que pretende andar numa direção e o servidor sabendo quanto é que a sua entidade consegue andar, irá validar com a sua energia restante se ainda tem energia para andar ou não.


No Update() do Unity, foi criado algo que serve para executar certas funcionalidades do Unity (Instanciate(), mudar a cor de uma imagem entre outras funcionalidades), que não é permitido correr numa Thread à parte e é necessário correr na main Thread do Unity. Foi criado para quando um jogador entra no servidor e este nos diz que entrou mais um jogador, para instanciar um GameObject de UI na lista de jogadores para sabermos quantos estão conectados de uma maneira visual.

  

__1) Instanciar e inicializar variáveis globais__, multiplas variáveis são insanciadas, estas que são utilizadas tanto para UI como para obter as prefabs dos jogadores, guardar a sua socket o seu ID entre outras coisas necessárias que serão modificadas conforme o decorrer do jogo.

  

__2) (Host) Iniciar o servidor__ Quando iniciado como host, é executado um processo que lança o servidor, este que está na pasta do client, e foi deixado com a opção de manter a janela a aparecer (mantida para debug e testes), mas com a possibilidade de remover a qualquer altura.

```
Process firstProc = new Process();
firstProc.StartInfo.FileName = ".\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
firstProc.EnableRaisingEvents = true;
firstProc.StartInfo.CreateNoWindow = false; <- true, para tirar a janela
firstProc.Start();
yield return new WaitForSeconds(2f);
```

Após lançar o servidor, o client espera 2 segundos utilizando o ```yield return new WaitForSeconds(2f);``` depois tenta conectar-se, verifica se não tem uma conexão efetuada e quantas tentativas foram efetuadas. Se não conseguir conectar, irá tentar 10 vezes com um intervalo de 1 segundo entre tentativas.

```
while (!isConnected && tryConn < 10)
{
	try
	{
		ConnectToServer();
	}
	catch
	{
		tryConn++;
		Thread.Sleep(1000);
	}
}
```

__3) Conectar ao servidor__, é utilizado o DNS para obter o endereço IP do servidor a juntar, que neste caso é o localhost, mas se fosse um google.pt, iria converter o seu nome para o devido IP. logo de seguida é utilizado o IP obtido e faz-se com que todos os clients que se juntem a utilizarem o IPv4, buscando o "AddressFamily.InterNetwork" em vez do "AddressFamily.InterNetworkV6". O endereço IP obtido previamente, é utilizado para a criação de um EndPoint, com o mesmo port que o servidor possui.

```
for (int i = 0; i < ipHost.AddressList.Length; i++)
	{
	if (ipHost.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
		{
			ipAddress = ipHost.AddressList[i];
		}
	}
	remoteEP = new IPEndPoint(ipAddress, 2100);
	socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
}
```

E também é criada a socket que fica associada ao servidor e à qual o client vai formar a conexão.

Com a conexão ao servidor efetuada, é criada uma Thread para começar a receber e enviar mensagens para o servidor.

```
receiveThread = new Thread(ReceiveData);
receiveThread.IsBackground = true;
receiveThread.Start();
```

__4) Receber dados do servidor__, tal como dito anteriormente, a obtenção dos dados vindos do servidor é efetuado da mesma maneira que o servidor tendo em conta o modelo RPC, enviando as duas mensagens, sendo o cumprimento da mensagem e a mensagem em sí, à maneira de colocar em Json e converter no objeto para depois usarmos os dados conforme necessário.

O ID do jogador é obtido assim que o client se conecta ao servidor, o servidor envia uma ação para o cliente com o nome "getid" e também o ID do jogador como parametro, irá ser invocada a respetiva função __AGetID()__, que atribui o valor fornecido pelo servidor ao cliente como o seu ID.

```
public void AGetID(int newID)
{
    RunOnMainThread(() =>
    {
        playerId = newID;
        if (playerId == 1)
            myIcon.color = new Color32(56, 56, 136, 255);
        else
            myIcon.color = new Color32(54, 156, 54, 255);

        DisplayID.text = playerId.ToString();
        ServerMessage($"Iam player {playerId}");

        for (int i = 0; i < playerId - 1; i++)
        {
            Instantiate(PlayerListPrefab, PlayerList);
        }
    });
}
```
  
Depois de todos os clientes já terem recebido os seus devidos IDs, o anfitrião pode começar o jogo, que irá enviar uma mensagem ao servidor com a ação "launch" que será verificada pelo servidor e respondido se o jogo vai começar ou se faltam jogadores. Se outro jogador tentar começar o jogo sem ser o anfitrião, o jogo irá informar que só o jogador 1 consegue lançar o jogo

Outra mensagem que os clientes podem receber é, se o jogo já comecou, o que faz com que comece para todos os clientes, escondendo a lista de jogadores onde está o botão para "Start Game" e mostrando os botões que o jogador pode carregar para mexer o seu personagem. Também começa por comparar o ID atual com o ID que vem do servidor para saber qual o jogador a jogar neste turno.

Após perguntarmos para lançar o jogo e se a resposta for positiva, o servidor envia a todos os jogadores a ação de "gamestart" que chama a função __AGameStart()__ e começando assim o jogo para todos os clientes
```
public void AGameStart(int playerTurn)
{
    RunOnMainThread(() =>
    {
        CleanTextBox();
        ServerMessage($"Game started! Player {playerTurn} turn.");
        RunOnMainThread(() => {turnIcon.color = new Color32(56, 56, 136, 255);});
        startGameScreen.SetActive(false);
        gameContols.SetActive(true);
        if (playerId == playerTurn)
        {
            ServerMessage("It's My turn :)");
            foreach (Button btn in gameButtons)
            {
                btn.interactable = true;
            }
        }
    });
}
```


__5) Enviar dados para o servidor__, é semelhante ao enviar dados do servidor para o cliente, convertendo a mensagem em Json, depois em bytes, verifica-se o seu tamanho e depois envia-se o cumprimento e logo de seguida a mensagem. Para tal foi criada a função __SendAction()__ para efetuar os envios das ações.

```
public void SendAction(string action = "", string target = "", int playerid = 0, string[] extra = null)
{
    if (extra == null)
        extra = new string[0];

    if (!isConnected) return;

    PlayerCommandSend msg = new PlayerCommandSend()
    {
        playerid = playerid,
        action = action.ToLower(),
        target = target.ToLower(),
        extra = extra
    };
    string json = JsonUtility.ToJson(msg); // using System.Text.Json
    byte[] data = Encoding.UTF8.GetBytes(json);
    byte[] dataLen = BitConverter.GetBytes((UInt32)data.Length);

    socket.Send(dataLen);
    socket.Send(data);
}
```

Conforme o jogador vai enviando ações para o servidor, este também vai receber ações para executar do seu lado, estas referentes ao movimento, ataque, ou mesmo só a atualização do seu UI, existem as seguintes ações vindas do servidor:


- __getid__, que chama a função AGetID() já vista anteriormente na obtenção do ID do cliente.
- __updateplayerlist__, que irá instanciar um novo cliente na lista visual de clientes
- __gamestart__, chama a função AGameStart() esta também vista anteriormente
- __canmove__, que recebe se pode ou não mexer do servidor, invoca a função ACanMove() e mexe o personagem do jogador a quantidade que veio do servidor.
```
public void ACanMove(int player, string direction, string[] amount)
{
    int amount0 = Convert.ToInt16(amount[0]);
    Vector3 movement = Vector3.zero;

    switch (direction)
    {
        case "up":
            movement = Vector3.forward * amount0;
            break;

        case "down":
            movement = Vector3.back * amount0;
            break;

        case "left":
            movement = Vector3.left * amount0;
            break;

        case "right":
            movement = Vector3.right * amount0;
            break;

        default:
            movement = Vector3.zero;
            break;
    }
    ChangePlayerEnergyBar(player, Convert.ToInt16(amount[1]));
    RunOnMainThread(() => playerPrefabs[player].transform.position += movement);
}
```
- __attack__, que chama a função AAttack() como o movimento, irá receber as informações que deve fazer, neste caso muda a sua barra de energia e de vida.
```
public void AAttack(string unit, int enemID, string[] amount)
{
    ChangeUnitHPBar(unit, enemID, Convert.ToInt16(amount[0]));
    ChangePlayerEnergyBar(Convert.ToInt16(amount[2]), Convert.ToInt16(amount[1]));
}
```
- __killed__ chama a função AKilled() e é muito parecida com a de attack, o que muda é a unidade que foi atacada perdeu toda a vida e esta é rodada 90º para parecer estar mosta.
```
public void AKilled(string unit, int enemID, string[] amount)
{
    ChangeUnitHPBar(unit, enemID, Convert.ToInt16(amount[0]));
    ChangePlayerEnergyBar(Convert.ToInt16(amount[2]), Convert.ToInt16(amount[1]));
    RunOnMainThread(() =>
    {
        Transform unitTrans = null;
        if (unit == "player")
            unitTrans = playerPrefabs[enemID].transform;
        else if (unit == "enemy")
            unitTrans = enemyPrefabs[enemID].transform;
        unitTrans.rotation = quaternion.Euler(90f, 0f, 0f);
    });
}
```
- __nextturn__, chama a função ANextTurn() e que recebe o turno do próximo jogador, verifica se é o seu e ativa todos os botões de ação se não estiverem e também muda no UI de todos os jogadores a cor do jogador a jogar.
```
public void ANextTurn(int nextPlayerID, string playerRefillEnergy)
{
    //infoBox.text += "\n----- NEW TURN -----";
    //ServerMessage("----- NEW TURN -----");
    ServerMessage($"\n----- Player {nextPlayerID} Turn! -----");
    RunOnMainThread(() =>
    {
        if (nextPlayerID == 1)
            turnIcon.color = new Color32(56, 56, 136, 255);
        else
            turnIcon.color = new Color32(54, 156, 54, 255);
    });

    ChangePlayerEnergyBar(nextPlayerID, Convert.ToInt16(playerRefillEnergy));
    if (playerId == nextPlayerID)
    {
        ServerMessage("It's My turn :)");
        RunOnMainThread(() =>
        {
            gameContols.SetActive(true);
            foreach (Button btn in gameButtons)
            {
                btn.interactable = true;
            }
        });
    }
}
```

Também foram criadas outras funções que são chamadas apenas por cada cliente, sendo estas o mudar o UI de cada jogador e que se aplica tanto à energia como à vida dos jogadores, estas funções vão buscar os sliders e modifica os seus valores para o valores vindos do servidor, no caso da vida restante ou da energia que ainda temos. Isto aplica-se à UI de cada prefab como também ao UI que temos no nosso ecrã que corresponde apenas ao nosso personagem.
- ChangePlayerEnergyBar() referente à barra de energia
```
public void ChangePlayerEnergyBar(int playerID, int energyLeft)
{
    RunOnMainThread(() =>
    {
        
        Slider[] sliders = playerPrefabs[playerID].GetComponentsInChildren<Slider>();
        foreach (Slider s in sliders)
        {
            if (s.name == "Energybar")
            {
                s.value = energyLeft;
                if (playerID == playerId)
                {
                    energySlider.value = energyLeft;
                }
            }
        }
    });
}
```
- ChangeUnitHPBar() referente à barra de vida
```
public void ChangeUnitHPBar(string unit, int playerID, int hpLeft)
{
    RunOnMainThread(() =>
    {
        Slider[] sliders = null;
        if (unit == "player")
            sliders = playerPrefabs[playerID].GetComponentsInChildren<Slider>();
        else if (unit == "enemy")
            sliders = enemyPrefabs[playerID].GetComponentsInChildren<Slider>();

        foreach (Slider s in sliders)
        {
            if (s.name == "HPbar")
            {
                s.value = hpLeft;
                if (playerID == playerId)
                {
                    healthSlider.value = hpLeft;
                }
            }
        }
    });
}
```

Referente ao movimento do personagem, existem vários botoes no ecrã de cada cliente, em que cada um deles envia uma ação para o servidor.
```
//Movement
public void goUp()
{
    SendAction("move", "up", playerId);
}

public void goDown()
{
    SendAction("move", "down", playerId);
}

public void goLeft()
{
    SendAction("move", "left", playerId);
}

public void goRight()
{
    SendAction("move", "right", playerId);
}
```

  

O mesmo se passa com o ataque, primeiro é necessário pressionar com o cursor uma unidade, logo de seguida carregar no botão "Attack" e será enviado para o servidor que queremos atacar a unidade selecionada.

```
public void OnAttackClicked()
{
    SendAction("attack", enemyName.text, playerId);
}
```

E o mais essencial para um jogo de turnos, o botão que envia o termino do próprio turno do jogador.

```
public void endTurn()
{
    SendAction("endturn");
}
```
## Outros 
Também foi criada uma função que quando o host vai para o Main Menu, que fecha o servidor em linha comandos devidamente.
```
public void StopServer()
{
    firstProc.Kill();
}
```
e outra para limpar a caixa de texto no caso de ser necessário.
```
public void CleanTextBox()
{
    infoBox.text = "";
}
```

# Análise de largura de banda
Dentro do programa do lado do servidor foram colocadas duas variáveis para armazenar os bytes que são tanto _enviados_ como _recebidos_, após fazer vários testes, e analizando o maior número de jogadas que um jogador pode fazer (neste caso andar 5 vezes numa direção), o máximo de bytes gasto numa ronda foi de __4217 bytes__ enviados e __780 bytes__ recebidos.
No caso deste jogo está desenhado apenas para 5 rondas, fazendo uma estimativa máxima do envio de dados de __21085 bytes__ e de __3900 bytes__ recebidos, dando um total de __24985 bytes__ enviados de entre o servidor e os clientes.

Analisando alguns sites e os seus preços, existe por exemplo a normcore.io que empresta servidres gratuitos até 120GB de banda larga por mês com o plano grátis (N.Public). mas se formos ves o plano (Pago) Pro que é 41.60€ por 3TB e este jogo apenas usa 24985 bytes, daria para milhões de partidas com este plano

# Limitações 
Neste caso como por o jogo online é uma das maiores limitações, da maneira que o jogo está criado, não tendo sido ponderado e criado desde inicio com esse intuito. mas no máximo quando se inicia o servidor este cria o endpoint para o "localhost", se estivesse a correr num servidor, seria possivel obter o IP do servidor, substituir pelo localhost e iniciar o servidor.
Por outro lado os Clientes também teriam de saber qual é esse IP para se poderem conectar e de momento não existe uma lista de servidores no jogo ou uma maneira de inserir o IP manualmente para se poder conectar.

# Diagrama de arquitectura de redes

![rede](https://imagizer.imageshack.com/img923/4817/rD7pkq.png)

  
  
  

## Bibliography

  

Sistemas de Redes para Jogos, material das aulas. Aula03

  

Aulas de Sistemas de Redes para Jogos 2023/24,

https://github.com/VideojogosLusofona/srj_2023_aulas/tree/main

  

Threads:

https://learn.microsoft.com/pt-pt/dotnet/standard/threading/using-threads-and-threading

  

Sockets:

https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0

https://discussions.unity.com/

JSON:
https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to
https://docs.unity3d.com/6000.1/Documentation/ScriptReference/JsonUtility.html