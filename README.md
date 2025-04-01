# Orion
## Téléchargement
Dans la catégorie **Releases**, cliquez sur la [dernière version](https://git.s2.rpn.ch/ComtesseE1/orion/-/releases/beta-0.0.0) et téléchargez le code source. Une fois installé sur votre machine, extrayez les fichiers dans un nouveau dossier. Vous allez devoir compiler vous même le programme pour le faire fonctionner. Pour ce faire, si vous ne le possédez pas déjà, récupérer le Kit de développement [Dotnet](https://dotnet.microsoft.com/en-us/download) et installez le sur votre machine. Une fois cela fait, vous pouvez exécuter le fichier *build.bat* se trouvant dans le répértoire *Desktop*. Cela effectuera une compilation avec des paramètres déjà prédéfinis pour les plateformes *Windows* et *Linux*. Vous pourrez ensuite retrouver les fichiers compilés dans le répértoire suivant : 
````
bin/Release/net9.0/win-x64/publish/
````
Si vous êtes sous linux, le remplacez le répértoire *win-x64* par *linux-x64*. Vous avez maintenant accès à l'exécutable et le logiciel se lance maintenant comme convenu. Son fonctionnement vous est expliqué au point suivant.
## Logiciel
Le logiciel fournit plusieurs fonctionnalités intéressantes. Premièrement, vous trouvez au centre de la plateforme circulaire un hologramme de la planète terre. Autour de celle-ci se trouve une représentation de la Station Spatiale Internationale qui se déplace en temps réel. 