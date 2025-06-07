# Sistemas de Redes para Jogos
### Micael Teixeira - 22208717
## PROJETO: Jogo online baseado em turnos, cliente/servidor, sem login/matchmaking

## Resumo:
O jogo criado não está a 100% implementado, porém a criação do servidor e ligação dos clientes com raw sockets está funcional (mesmo não tendo o melhor código ou a maneira mais correta de fazer várias coisas).

## Descrição técnica:
Foram utilizadas as __Raw Sockets__ com o protocolo __TCP__ para criar as ligações do servidor e dos clients. Foi escolhido fazer o projeto com o protocolo TCP, por este exigir as conexões como deve ser e porque garante que as informações ao serem enviadas serão entregues e que quase garantidamente chegaram sem erros, e também pela estabilidade e por não se querer perder mensagens sendo estas importantes ou não.

A __desvantagem__ do protocolo TCP é que este poderá chegar ao ponto de ficar bastante mais lento se o fluxo de informação enviada for maior, visto que este projeto era para ser algo pequeno. A possivel perda de dados do protocolo UDP e a sua ordem "aleatoria" de chegada da informação não ajudava com o que se queria desenvolver, possivelmente dando origem a multiplos futuros erros e uma grande perda de tempo para os corrigir. Mesmo sendo mais rápido, seria preciso fazer muitas mais verificações, fazendo com que a sua rapidez acabasse por não compensar. 

O __Multithread__ também foi aprendido e utilizado para que quando existisse uma ligação ao servidor, este fosse aceite e inserido no seu próprio thread, fazendo com que os seus inputs e dados, sejam tratados independentemente, facilitando assim as várias conexões ao servidor e permitindo que cada client mexa no seu jogo sem impactar os outros ou com que a aplicação bloqueie à espera que seja o seu turno.     

A interpretação de comandos e conexões, tanto do servidor como do cliente tem haver com as __mensagens enviadas__ do cliente/servidor ou servidor/cliente são enviadas e recebidas como strings de texto normal, podendo ou não conter várias separações contendo dois pontos ":", permitindo enviar vários dados como por exemplo o jogador1 pedir para andar para baixo enviando ("1:move:down"), estes dados serão recebidos e processados pelo servidor, fazendo a separação da mensagem toda por partes onde forem encontrados os dois pontos ":", depois o servidor irá responder com uma mensagem adequada, sendo apenas informativa ou com dados para o cliente processar. 

Foi criada uma __classe Entity.cs__ no servidor, para que cada jogador e inimigo tenha a sua vida, energia, movimento e o dano. Esta classe foi criada para facilitar a gestão de cada entidade dentro do jogo, permitindo saber os seus dados e fazer a gestão dos mesmos conforme atualizados pelo servidor.

## O funcionamento do servidor começa por:
__1) Instanciar e inicializar variáveis globais__, desde uma lista de clients, outra lista com os IDs dos jogadores, turno, rondas, número máximo de jogadores e também cria as instancias dos jogadores, estas que serão modificadas conforme o decorrer do jogo com os dados do client.
 
 __2) Criação do endpoint e de um listener__, começa-se por criar uma socket 
```
Socket  listener  =  new  Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
``` 
Esta socket usa o IPv4 e é do tipo Stream e com o protocolo TCP, fazendo com que se obtenha um fluxo contínuo de dados com entrega garantida e ordenada dos dados. Logo de seguida é criado o EndPoint, onde os clientes se vão juntar, tendo este um port (2100) e um IP em que estamos a utilizar o localhost, este permite qualquer conexão local.
``` 
IPEndPoint(IPAddress.Any, 2100);
listener.Bind(localEndPoint);
listener.Listen(maxPlayers); 
``` 

Fazendo o .Bind() para ativar a socket comforme o IP e o port fornecido e depois o .Listen(maxPlayers), que serve para escutar as conexões dos clients até um número dados, sendo este o maxPlayers, faz com que apenas o número máximo de players seja permitido conectar-se, e fazendo com que os restantes não consigam estabelecer uma conexão com o servidor.

__3) Ouvir e aceitar conexões__, após o servidor estar à espera de conexões de clients, é efetuado um ciclo que fará com que cada conexão recebida, seja aceite e de seguida adicionada a uma lista de sockets chamada clients, que será utilizada para chamar o respetivo jogador e enviar informação e também atribuindo o número de chegada como o ID do jogador. No final é criado uma Thread para a nova conexão, fazendo com que se trate de todos os jogadores ao mesmo tempo.
``` 
listener.Accept();
clients.Add(client);
assignedId  =  clients.Count;
playerIds.Add(assignedId);
Thread  clientThread  =  new  Thread(() =>  HandleClient(client, assignedId));
clientThread.Start();
Broadcast($"Player {assignedId} connected, updatePlayerList ");
``` 
__4) Tratamento individual de um cliente__, é efetuado com o iniciar a Thread criada anteriormente, e que começa por preparar um buffer de recepção de dados que pode receber até 64 bytes, visto que apenas serão enviadas strings e que é pouco provavel chegar ao limite escolhido.
``` byte[] buffer  =  new  byte[64]; ``` 

__4.1) Receber informações dos clients__, o receber informações é o mais importante deste jogo, pois todas as ações do jogador são feitas atravez do envio e da recepção de mensagens. O servidor começa por estar num ciclo que corre enquanto o client estiver com a sua socket ligada e enquanto o número de rondas for inferior ao valor estipulado. 
Enquanto que para enviar mensagens convertemos em bytes, para receber convertemos esses mesmos bytes em string.
``` string msg = Encoding.ASCII.GetString(buffer, 0, bytesRead); ```

Após receber e decodificar a mensagem, obtemos uma string com as informações vindas dos clients e que será explicada a sua utilização futuramente.


__4.2) Enviar informações para os clients__, existem duas maneiras de enviar informações aos clients, uma que envia apenas ao próprio client Send(), (no caso de tentar andar sem ser o turno dele etc...), a mensagem será convertida de string para bytes e enviada utilizando a socket fornecida. 
``` 
static  void  Send(Socket  client, string  message)
{
	byte[] data  =  Encoding.ASCII.GetBytes(message+"\n");
	client.Send(data);
}
``` 
Também é possivel enviar uma mensagem para todos os clients, em que se baseia na função Send(), só que em vez de receber um socket, irá percorrer a lista de clients que é uma lista de sockets, e para cada um deles irá enviar a mensagem.

__4.3) Processamento das mensagens recebidas__, após recebermos a mensagem esta é separada em partes para dentro de um array, sendo o critério de separação os dois pontos ":", isto sendo feito tanto no servidor como nos clients para se obter uma coerencia entre os sistemas.
As mensagens recebidas do client vem sempre com uma estrutura bem definida, sendo esta: __ID:Action:Optional.__
E que se resume às possiveis escolhas do jogador, estas estão divididas em __launch__, usado para começar o jogo, __move,__ que dada uma direção irá verificar se o jogador pode andar conforme a sua energia, __attack__ que serve para futuramente efetuar um ataque a uma outra entidade, e por fim __endTurn__ em que o servidor termina o turno de um jogador e dá indicação de qual será o próximo a jogar. 

Para ser possível lançar o jogo, este tem de ser o Host, ou seja, o primeiro jogador a conectar-se ao servidor e também é necessário ter todos os jogadores dentro do servidor, caso não aconteça o servidor alerta ao host que ainda faltam jogadores.
```
if (parts[1] ==  "launch"  &&  playerId  ==  1  &&  !gameStarted  &&  clients.Count  ==  maxPlayers)
{
	gameStarted  =  true;
	Broadcast($"Game started! It's player{playerIds[currentTurn]} Turn:{playerIds[currentTurn]}");
	continue;
}
else  if (parts[1] ==  "launch"  &&  playerId  ==  1  &&  !gameStarted  &&  clients.Count  !=  maxPlayers)
{
	Send(client, $"Need more players - {clients.Count}/{maxPlayers}");
	continue;
}
```

Assim que um jogador se conecta ao servidor é logo indicado que está à espera do Host para começar o jogo.
```
if (!gameStarted)
{
	Send(client, "Waiting for Host to start the Game.");
	continue;
}
```
No caso de um problema em que o Host tenha lançado o jogo mas que ainda seja possivel tentar começar o jogo, irá enviar uma mensagem a confirmar que o jogo já está iniciado. Este código apenas se aplica ao Host, visto que cada client tem a sua validação se é o host ou não e que permite com que este inicie o jogo, ou não.
```
if (parts[1] ==  "launch"  &&  gameStarted)
{
	Send(client, "Game already started");
	continue;
}
```
Se um client tentar jogar ou fazer alguma interação antes de ser o seu turno, será mandada uma mensagem a esse client a alertar que ainda não é o seu turno.
```
if (playerIds[currentTurn] !=  playerId)
{
	Send(client, "Not your turn.");
	continue;
}
```
Apenas com o iniciar do jogo, o servidor agora começa a poder processar as ações dos clients, recebendo o movimento, os ataques e quando estes querem terminar o seu turno. No exemplo do jogador querer andar, irá enviar ao servidor que pretende andar numa direção e o servidor sabendo quanto é que a sua entidade consegue andar, irá validar com a sua energia restante se ainda tem energia para andar ou não.

Validar o movimento de um personagem:
```
if (parts[1] ==  "move")   //parts[1] é a ação
{
	if (parts[0] ==  "1")   //parts[0] é o playerID
	{
		if (player1Pre.getEnergy() >=  20)
		{
			player1Pre.takeEnergy(20);
			Broadcast($"CanMove:{parts[2]}:{player1Pre.getMovement()}:{player1Pre.getEnergy()}:{parts[0]}");
		}
		else
		Send(client, $"No Energy:{player1Pre.getEnergy()}");
  ...
```

Execução de um ataque, em que verifica que recebemos "attack" do client e ataca um dos inimigos (Para já inacabado pois falta receber o inimigo e enviar mensagens para o client atualizar o estado do inimigo, vida, animações etc...).
```
if (parts[1] ==  "attack")
{
	if (1  ==  Convert.ToInt16(parts[0]))
		player1Pre.Attack(enemy1);
	if (2  ==  Convert.ToInt16(parts[0]))
		player2Pre.Attack(enemy1);
}
```

E para terminar o receber de dados de um client, temos a mensagem "endTurn" que irá verificar quantos jogadores têm a partida e depois aumentar o turno, se voltar ao primeiro jogador, uma ronda acabou e será indicado aos jogadores, tanto que a ronda acabou como quem é o próximo jogador. Entretanto antes de acabar o turno irá recarregar a energia do jogador. 
```
if (parts[1] ==  "endTurn")
{
	if (maxPlayers  ==  2) //Need to fix this
	{
		if (currentTurn  ==  0)
			currentTurn++;
		else
		{
			currentTurn--;
			Broadcast("Round: "  +  ++rounds);
		}
	}
	else
	{
		currentTurn  = (currentTurn  +  1) %  playerIds.Count  -  1;
		if ((currentTurn  +  1) %  playerIds.Count  -  1  ==  0)
		{
			Broadcast("Round: "  +  ++rounds);
		}
	}
	player1Pre.refilEnergy();
	player2Pre.refilEnergy();
	Broadcast($"Next turn ({currentTurn}): {playerIds[currentTurn]}");
}
```

Após uma ação válida tenha sido executada do lado do servidor, é enviada uma mensagem para todos os clientes a dizer o que sucedeu.
```
if (parts.Length  >  2)
	Broadcast($"{playerId}:{parts[1]}:{parts[2]}");
else
	Broadcast($"{playerId}:{parts[1]}:{""}");
```
No final de tudo estar concluido, todas as ligações das sockets dos clientes são fechadas e removidas das listas. São removidos da lista de clients e da lista de IDs, como também faz com que a comunicação às sockets de enviar e receber dados seja impossivel. 
```
clients.Remove(client);
if (playerId  !=  -1) playerIds.Remove(playerId);
client.Shutdown(SocketShutdown.Both);
client.Close();
```

## O funcionamento do Client/Host começa por:

No metodo Awake() do unity, foram feitas configurações para retirar o fullscreen e baixar as frames (por possiveis problemas de sobreaquecimento), cada cliente terá os seus botões para controlar o personagem desativados e escondidos assim que se tenta juntar ao servidor

```
QualitySettings.vSyncCount  =  1;
Application.targetFrameRate  =  30;
Screen.SetResolution(1280, 720, false);
Screen.fullScreen  =  false;
gameContols.SetActive(false);
foreach (Button  btn  in  gameButtons)
{
	btn.interactable  =  false;
}
```

No Update() do Unity, foi criado algo que serve para executar certas funcionalidades do Unity (Instanciate()), que não é permitido correr numa Thread à parte e é necessário correr na main Thread do Unity. Foi criado para quando um jogador entra no servidor e este nos diz que entrou mais um jogador, para instanciar um GameObject de UI na lista de jogadores para sabermos quantos estão conectados de uma maneira visual. 

__1) Instanciar e inicializar variáveis globais__, multiplas variáveis são insanciadas, estas que são utilizadas tanto para UI como para obter as prefabs dos jogadores, guardar a sua socket o seu ID entre outras coisas necessárias.

__2) (Host) Iniciar o servidor__ Quando iniciado como host, é executado um processo que lança o servidor, este que está na pasta do client, este foi deixado com a opção de manter a janela a aparecer (mantida para debug e testes), mas com a possibilidade de remover a qualquer altura.
```
Process  firstProc  =  new  Process();
firstProc.StartInfo.FileName  =  ".\\SocketServer\\bin\\Debug\\net8.0\\SocketServer.exe";
firstProc.EnableRaisingEvents  =  true;
firstProc.StartInfo.CreateNoWindow  =  false;
firstProc.Start();
Thread.Sleep(2000);
```
Após lançar o servidor, o client espera 2 segundos e depois tenta conectar-se, verifica se não tem uma conexão efetuada e quantas tentativas foram efetuadas. Se não conseguir conectar, irá tentar 10 vezes com um intervalo de 1 segundo entre tentativas.
```
while (!isConnected  &&  tryConn  <  10)
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

__3) Conectar ao servidor__,  é utilizado o DNS para obter o endereço IP do servidor a juntar, que neste caso é o localhost, mas se fosse um google.pt, iria converter o seu nome para o devido IP. logo de seguida é utilizado o IP obtido e faz-se com que todos os clients que se juntem a utilizarem o IPv4, buscando o "AddressFamily.InterNetwork" em vez do "AddressFamily.InterNetworkV6". O endereço IP obtido previamente, é utilizado para a criação de um EndPoint, com o mesmo port que o servidor possui.
```
for (int  i  =  0; i  <  ipHost.AddressList.Length; i++)
{
	if (ipHost.AddressList[i].AddressFamily  ==  AddressFamily.InterNetwork)
	{
		ipAddress  =  ipHost.AddressList[i];
	}
}
remoteEP  =  new  IPEndPoint(ipAddress, 2100); 
socket  =  new  Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
```

E também é criada a socket que fica associada ao servidor e à qual o client vai formar a conexão.
Existindo a conexão, é encapsulada a socket com uma Stream, para enviar e receber os dados e só ai é que se confirma que a conexão está estabelecida.
```
socket.Connect(remoteEP);
stream  =  new  NetworkStream(socket);
isConnected  =  true;
```
Com a conexão ao servidor efetuada, é criada uma Thread para começar a receber e enviar mensagens para o servidor.
```
receiveThread  =  new  Thread(ReceiveData);
receiveThread.IsBackground  =  true;
receiveThread.Start();
```
__4) Receber dados do servidor__, tal como o servidor é criado um buffer de 64 bytes para receber os dados do servidor, sendo suficiente para o existente e ainda permitindo expansão do código para enviar mais informação se necessário. Os dados no entanto são recebidos como bytes utilizando a stream em vez da socket, que por sua vez são convertidos em string e depois adicionados a uma caixa de texto que serve como log das informações passadas e recebidas.

Como já falado, os dados recebidos são partidos em várias partes atravez da separação utilizando os dois pontos ":" e que as partes resultantes, tal como o servidor são verificadas se correspondem a algo no código para percorrer.
O Player ID é obtido assim que o client se liga ao servidor, o servidor manda uma mensagem que trás o PlayerID : x : ... e este é separado e associado à variável PlayerID do cliente, fazendo com que este obtenha o seu ID e faz com que este mostre no UI.

```
if (msg.Contains("PlayerID"))
{
	int  incomeID  =  Convert.ToInt16(msg.Split(':')[1]);
	RunOnMainThread(() =>
	{
		playerId  =  incomeID;
		DisplayID.text  =  playerId.ToString();
		infoBox.text  +=  $"\nIam player {playerId}"; 
		for (int  i  =  0; i  <  playerId  -  1; i++)
		{
			Instantiate(PlayerListPrefab, PlayerList);
		}
	});
}
```

Assim que um novo client se junda ao servidor, é lançada uma mensagem para todos os clients para adicionarem ao seu Unity UI um prefab representativo de um novo jogador.
```
else  if (msg.Contains("updatePlayerList"))
{
	infoBox.text  +=  "MY ID: "  +  playerId;
	RunOnMainThread(() =>  Instantiate(PlayerListPrefab, PlayerList));
}
```
Outra mensagem que os clientes podem receber é, se o jogo já comecou, o que faz com que comece para todos os clientes, escondendo a lista de jogadores onde está o botão para "Start Game" e mostrando os botões que o jogador pode carregar para mexer o seu personagem. Também começa por comparar o ID atual com o ID que vem do servidor para saber qual o jogador a jogar neste turno.
```
else  if (msg.Contains("Game started"))
{
	startGameScreen.SetActive(false);
	gameContols.SetActive(true);
	infoBox.text  +=  "\nGame started! Player "  +  msg.Split(':')[1] +  "'s turn.";
	if (playerId  ==  Convert.ToInt16(msg.Split(':')[1]))
	{
		infoBox.text  +=  "\nIt's My turn :)";
		foreach (Button  btn  in  gameButtons)
		{
			btn.interactable  =  true;
		}
	} 
	playerPosition.transform.position  =  transform.position;
}
```

Entretanto também existem mensagem vindas do servidor que remetem para coisas que o jogador não pode fazer, ou que não tem controlo, tal como o não ser o turno do jogador.
```
else  if (msg.Contains("Not your turn"))
{
	infoBox.text  +=  "\n"  +  msg;
}
```
Ou o jogador querer andar e não ter energia suficiente.
```
else  if (msg.Contains("No Energy"))
{
	//Already writes the message
}
```
Ou então o estar à espera que o Host começe o jogo
```
else  if (msg.Contains("Waiting for Host"))
{
	infoBox.text  +=  "\n"  +  msg;
}
```

Entretanto depois de garantir que a mensagem que veio não é nenhuma indesejada, também é tratado o movimento do jogador, recebendo "CanMove" e a direção na mesma mensagem, irá ser mudada a posição do prefab do jogador conforme a velocidade que este possui no servidor.
```
else  if (msg.Contains("CanMove"))
{
	infoBox.text  +=  "\n"  +  msg;
	if (msg.Split(':')[1] ==  "up")
{
	playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position  =  new  Vector3(
	playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.x,
	playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.y,
	playerPrefabs[Convert.ToInt16(msg.Split(':')[4])].transform.position.z  +  Convert.ToInt16(msg.Split(':')[2]));
	}
  ...
}
```
E para terminar o receber e tratar mensagens, temos a mensagem "Next turn" que quando recebida pelo servidor, irá fazer uma verificação se o ID do turno corresponde ao ID do jogador, fazendo assim com que seja o seu turno, ou não. Sendo o turno do jogador, serão ativados os botões de interação que enviam informações ao servidor.
```
else  if (msg.Contains("Next turn"))
{
	infoBox.text  +=  "\n----- NEW TURN -----";
	if (playerId  ==  Convert.ToInt16(msg.Split(':')[1]))
	{
		gameContols.SetActive(true);
		infoBox.text  +=  "\nIt's My turn :)";
		foreach (Button  btn  in  gameButtons)
		{
			btn.interactable  =  true;
		}
	}
}
```
Entretanto enquanto o código corre também existe a tentativa de obtenção de erros com try catch sendo estas na conexão ao servidor. ConnectToServer()
``` 
try
{
	...
}
catch (Exception  ex)
{
infoBox.text  +=  "\nError connecting to server: "  +  ex.Message;
}
```
Como também na recepção de dados. ReceiveData() 
```
try
{
	...
}
catch (SocketException  e)
{
	if (e.SocketErrorCode  !=  SocketError.WouldBlock)
	{
		throw  e;
	}
}
catch (Exception  e)
{
	Console.WriteLine("Error getting data: "  +  e.Message);
}
```

__5) Enviar dados para o servidor__, Antes de enviar dados para o servidor verifica-se se o cliente ainda está conectado, após a validação, compõe-se a mensagem com o ID do jogador, a ação a realizar e a opcional. Após a mensagem estar criada, é converida em Bytes e com o auxílio da Stream, envia-se.
```
public  void  SendAction(string  action, string  target  =  "")
{
if (!isConnected) return;
string  message  =  $"{playerId}:{action}:{target}";
byte[] data  =  Encoding.ASCII.GetBytes(message);
stream.Write(data, 0, data.Length);
infoBox.text  +=  "\nSent: "  +  message;
}
```
Dentro do enviar dados, temos as funções do botões que servem tanto para lançar o jogo como o movimento, ataque e o terminar o turno.

Quando se tenta carrega para lançar o jogo, se não for o Host, é lançada uma verificação a dizer que o jogador em questão não pode começar o jogo.

```
public  void  OnLaunchClicked()
{
	if (playerId  ==  1)
	{
		SendAction("launch");
	}
	else
	{
		infoBox.text  +=  "\nOnly Player 1 can launch the game.";
	}
}
```
Referente ao movimento do personagem, existem vários botoes e para cada um deles a sua informação a ser enviada.
```
public  void  goUp()
{
	SendAction("move", "up");
}

public  void  goDown()
{
	SendAction("move", "down");
}

public  void  goLeft()
{
	SendAction("move", "left");
}

public  void  goRight()
{
	SendAction("move", "right");
}
```

O mesmo se passa com o ataque, só que ainda falta conseguir enviar a entidade a atacar, a ideia existe mas não está completa.
```
public  void  OnAttackClicked()
{
	// Just attacks enemy1
	SendAction("attack");
}
```
E o mais essencial para um jogo de turnos, o terminar o próprio turno do jogador.
```
public  void  endTurn()
{
	SendAction("endTurn");
}
```

# Diagrama de arquitectura de redes
![rede](https://imagizer.imageshack.com/img923/7040/t7NXg5.jpg)



 
## Bibliography

Sistemas de Redes para Jogos, material das aulas. Aula03

Aulas de Sistemas de Redes para Jogos 2023/24,
https://github.com/VideojogosLusofona/srj_2023_aulas/tree/main

Threads:
https://learn.microsoft.com/pt-pt/dotnet/standard/threading/using-threads-and-threading

Sockets:
https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket?view=net-9.0
https://discussions.unity.com/