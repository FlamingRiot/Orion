# Orion
## Websummary
### Description
Dans le cadre de mon TPI, j’ai conçu une application de visualisation avancée en 3D capable
de communiquer avec un automate programmable développé par la filière *automatique* du
CPNE-TI. L’objectif de ce dernier est de faire bouger une sphère à l’aide de moteurs pas-à-pas,
afin que celle-ci indique au moyen d’un repère la direction d’un objet choisi dans le ciel.

Mon travail a été de programmer le logiciel de visualisation et de calculer les données
nécessaires à envoyer aux moteurs avec la librairie graphique Raylib en C#.

### Objectifs

 - Comprendre et interpréter des données astronomiques et géographiques afin de
déterminer une orientation réaliste vers la position en question, le tout dans un repère
tridimensionnel.
 - Mettre en place des moyens de visualisation innovants dans un environnement 3D à
l’aide d’une librairie graphique simple et légère.
 - Appliquer les principes d’intégration continue étudiés durant la formation et créer un
livrable complet pour les utilisateurs.
 - Comprendre les principes du protocole Websocket et les mettre en application afin de
communiquer avec un automate programmable non-documenté.

### Réalisation

Le projet a débuté sur la base de l’application déjà existante réalisée par mes soins durant la
saison précédente. Les périodes de TPI ont donc été consacrées à l’ajout de fonctionnalités
cruciales et à l’aboutissement technique du logiciel.
J’ai finalisé le projet par la liaison de l’application à l’automate programmable afin de mettre à
profit les simulations virtuelles présentes dans l’application.
Une attention toute particulière a été portée à l’aspect graphique du logiciel. En faisant usage
de Shaders GLSL et d’algorithmes poussés, je suis parvenu à donner un visuel professionnel
et abouti à l’environnement 3D.

## Mise en place
### Téléchargement

Vous trouvez ci-dessous la marche à suivre permettant de mettre en place le projet dans des conditions minimales, c'est-à-dire sur une machine vierge de tout composant du logiciel. Il vous est également expliqué comment connecter l'automate et le PC au travers d'un routeur. 

Tout d'abord, rendez-vous dans les [Releases](https://git.s2.rpn.ch/ComtesseE1/orion/-/releases) du dépôt Gitlab. Téléchargez l'exécutable de la dernière version publiée tel qu'il est mis à disposition et extrayez le sur votre machine. Toutefois, une fois extrait, le logiciel ne pourra fonctionner dans l'état fourni. Les restrictions Gitlab empêchant la sauvegarde en artéfacts de fichiers trop volumineux, il était impossible de joindre les *assets* de l'application avec la release. C'est pourquoi vous devez également cloner le dépôt principal chez vous afin de récupérer le dossier *assets*, puis le copier dans le répertoire de l'exécutable. Sa localisation dans le dépôt vous est donnée : 
````
/desktop/assets
````

Vous devez au final vous retrouver avec l'arborescence suivante :

````
.
|
|--- Orion-Desktop.exe
|
|--- raylib.dll
|
|--- assets
````

Dès à présent, vous êtes en mesure de démarrer l'aplication sans qu'aucune compilation ne soit nécessaire. Toutefois, si vous souhaitez effectuer l'installation complète qui comprend l'automate programmable, il reste encore à y connecter votre PC afin que le logiciel puisse communiquer avec. 

### Connexion à l'automate

Pour commencer, démarrez le système à l'aide de l'interrupteur orange placé à l'arrière du boîtier : 

![Interrupteur de l'automate (allumé)](desktop/assets/README/Switch_Source.jpg)

Il lui faut ensuite quelques minutes pour démarrer tout ses composants. Vous
pouvez profiter de ce temps pour brancher le routeur à une prise électrique, puis
placer les câbles Ethernet de la manière suivante :

![Configuration du routeur](desktop/assets/README/Router_Physical.png)

Vous aurez peut-être remarqué plusieurs câbles Ethernet branchés à l’automate
et vous vous demandez peut-être lequel brancher au routeur. Utilisez celui-ci :

![Câble Ethernet à utiliser sur l'automate](desktop/assets/README/Ethernet_Source.jpg)

Pour terminer, il vous reste encore à configurer le routeur de sorte à ce qu’il
distribue les adresses correctement entre le robot et l’ordinateur grâce au
protocole DHCP. Pour ce faire, commencez par effectuer un redémarrage du
routeur Netgear en maintenant enfoncé pendant 10 secondes minimum le bouton
« reset » à l’arrière. Puis, rendez-vous dans un navigateur Internet et saisissez
l’adresse suivante : 192.168.1.1. Il vous sera alors proposé un certain type de
configuration automatique ; cliquez sur Non. Vous pouvez ensuite vous rendre à
l’adresse routerlogin.net/start.htm, depuis laquelle vous allez configurer la
distribution des adresses IP entre les différents clients.

Rendez-vous dans : **Avancé -> Configuration -> Paramétrage LAN** et entrez les
données suivantes, de sorte à ce qu’elles correspondent à l’adresse statique de
l’automate :

<p align="center">
    <img src="desktop/assets/README/Router_Config.png">
</p>

Vous pouvez dès à présent démarrer l’application à l’aide de l’exécutable et la
connexion devrait s’établir dans les 15 premières secondes. Si toutefois vous
obtenez un message d’erreur dans la console du terminal de contrôle, il s’agit
probablement qu’une autre connexion avec le Websocket est déjà ouverte
quelque part sur votre machine. Dans ce cas, fermez toutes les interfaces Web
susceptibles d’y être connectées.