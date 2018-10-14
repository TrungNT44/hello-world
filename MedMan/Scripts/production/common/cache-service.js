app.instance.factory('cacheService', cacheService);
cacheService.$inject = ['CacheFactory'];

function cacheService(CacheFactory) { // Create cache service
    if (!CacheFactory.get('cacheService')) {
        CacheFactory.createCache('cacheService', {
            maxAge: 3600000,
            deleteOnExpire: 'aggressive',
            recycleFreq: 60000,
            storageMode: 'localStorage' // This cache will use `localStorage`. -> Data is not cleared when the page is refreshed.
        });
    }

    var cacheInst = CacheFactory.get('cacheService');
    return {
        cacheInstance: function () {
            return cacheInst;
        }
    };
};