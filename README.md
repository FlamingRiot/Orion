# Orion
Ce projet est un projet de TPI et PreTPI. Il prendra fin le 23 mai 2025, et est en développement depuis le 27 Janvier 2025. Pour donner un bref contexte, le but de l'application est de pouvoir dans sa version finale communiquer avec un robot et ainsi le faire pointer en temps réel l'ISS en fonction de l'endroit sélectionné sur Terre (ou par le biais d'une localisation GPS). L'application permet également une visualisation avancée en 3D.
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