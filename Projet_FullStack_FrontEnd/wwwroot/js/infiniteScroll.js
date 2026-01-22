// Infinite Scroll pour le Pokédex
window.initInfiniteScroll = (dotNetHelper) => {
    let isLoading = false;
    let scrollTimeout = null;

    const handleScroll = () => {
        // Annuler le timeout précédent pour débounce
        if (scrollTimeout) {
            clearTimeout(scrollTimeout);
        }

        scrollTimeout = setTimeout(() => {
            if (isLoading) return;

            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            const windowHeight = window.innerHeight;
            const documentHeight = document.documentElement.scrollHeight;

            // Charger plus quand on est à 80% du bas de la page
            const scrollPercentage = (scrollTop + windowHeight) / documentHeight;

            if (scrollPercentage > 0.8) {
                isLoading = true;
                dotNetHelper.invokeMethodAsync('LoadMorePokemons')
                    .then(() => {
                        isLoading = false;
                    })
                    .catch((error) => {
                        console.error('Erreur lors du chargement des pokémons:', error);
                        isLoading = false;
                    });
            }
        }, 200); // Débounce de 200ms
    };

    // Ajouter l'écouteur d'événement
    window.addEventListener('scroll', handleScroll, { passive: true });

    // Fonction de nettoyage (appelée lors du démontage du composant)
    return {
        dispose: () => {
            window.removeEventListener('scroll', handleScroll);
            if (scrollTimeout) {
                clearTimeout(scrollTimeout);
            }
        }
    };
};
