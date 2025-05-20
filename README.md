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

## Téléchargement
Dans la catégorie **Releases**, cliquez sur la [dernière version](https://git.s2.rpn.ch/ComtesseE1/orion/-/releases/beta-0.0.0) 
et téléchargez le code source. Une fois installé sur votre machine, extrayez les fichiers dans un nouveau dossier. Vous allez 
devoir compiler vous même le programme pour le faire fonctionner. Pour ce faire, si vous ne le possédez pas déjà, récupérer le 
Kit de développement [Dotnet](https://dotnet.microsoft.com/en-us/download) et installez le sur votre machine. Une fois cela fait, 
vous pouvez exécuter le fichier *build.bat* se trouvant dans le répértoire *Desktop*. Cela effectuera une compilation avec des 
paramètres déjà prédéfinis pour les plateformes *Windows* et *Linux*. Vous pourrez ensuite retrouver les fichiers compilés dans 
le répértoire suivant : 
````
bin/Release/net9.0/win-x64/publish/
````
Si vous êtes sous linux, remplacez le répértoire *win-x64* par *linux-x64*. Vous avez maintenant accès à l'exécutable et le logiciel se lance comme convenu. Son fonctionnement vous est expliqué au point suivant.
## [Logiciel]
Le logiciel fournit plusieurs fonctionnalités intéressantes. Premièrement, vous trouvez au centre de la plateforme circulaire 
un hologramme de la planète terre. Autour de celle-ci se trouve une représentation de la Station Spatiale Internationale se 
déplaçant en temps réel dont vous pouvez apprécier le mouvement aussi longtemps que vous le souhaitez. Puis, sur le bord 
du cercle, vous pouvez apercevoir un terminal holographique. Celui-ci sert à communiquer avec l'automate physiquement présent 
à côté de l'ordinateur. Vous avez la possibilité d'y modifier le point de vue de la simultion en coordonées géographiques, 
qui servent à la simulation virtuelle ainsi que physique avec l'automate. Vous avez également la possibilité de consulter 
les informations en temps réel de l'object sélectionné, ainsi que de modifier celui-ci pour pointer celui de votre choix.
Au sommet de la sphère holographique qui se trouve au centre de la salle, vous pouvez également trouver une flèche bleutée. 
Celle-ci correspond à la simulation virtuelle de ce à quoi devrait ressembler cette de l'automate dans le monde réel. Elle 
se base sur le point de vue sélectionné, et part du principe qu'elle se trouve à la surface de la terre. Pour vous orienter et 
ainsi vérifier par vous-même si la simulation de l'automate fonctionne, se trouve en haut de l'écran une boussole virtuelle 
qui permet ainsi d'évaluer l'orientation de la flèche virtuelle. 

[Logiciel]: CHANGELOG.md