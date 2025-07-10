// By Zihad - HydraHD Cloudstream Extension

package HydraHD

import com.lagradost.cloudstream3.*
import com.lagradost.cloudstream3.MainAPI
import com.lagradost.cloudstream3.TvType
import com.lagradost.cloudstream3.utils.loadExtractor
import org.jsoup.Jsoup

class HydraHD : MainAPI() {
    override var name = "HydraHD"
    override var mainUrl = "https://hydrahd.sh"
    override var lang = "en"
    override val supportedTypes = setOf(TvType.Movie, TvType.TvSeries)

    override val mainPage = mainPageOf(
        "$mainUrl/movies/" to "Movies",
        "$mainUrl/tv-series/" to "TV Shows",
        "$mainUrl/anime/" to "Anime"
    )

    override suspend fun getMainPage(
        page: Int,
        request: MainPageRequest
    ): HomePageResponse {
        val doc = app.get(request.data).document
        val items = doc.select("div.ml-item")
            .mapNotNull {
                val href = it.selectFirst("a")?.attr("href") ?: return@mapNotNull null
                val title = it.selectFirst("img")?.attr("alt") ?: "No title"
                val poster = it.selectFirst("img")?.attr("src")
                val isTv = href.contains("/tv-series/") || href.contains("/episode-")
                MovieSearchResponse(
                    title,
                    href,
                    this.name,
                    TvType.Movie,
                    poster,
                    year = null
                )
            }
        return newHomePageResponse(request.name, items)
    }

    override suspend fun load(url: String): LoadResponse? {
        val doc = app.get(url).document
        val title = doc.selectFirst("meta[property=og:title]")?.attr("content") ?: "No title"
        val poster = doc.selectFirst("meta[property=og:image]")?.attr("content")

        val videoUrl = doc.selectFirst("iframe")?.attr("src")
            ?: return null

        val sources = mutableListOf<ExtractorLink>()

        loadExtractor(videoUrl, mainUrl, sources)

        return MovieLoadResponse(
            title,
            url,
            this.name,
            sources,
            poster = poster,
            year = null
        )
    }

    override suspend fun search(query: String): List<SearchResponse> {
        val res = app.get("$mainUrl/?s=$query").document

        return res.select("div.ml-item a").mapNotNull {
            val href = it.attr("href")
            val title = it.selectFirst("img")?.attr("alt") ?: return@mapNotNull null
            val poster = it.selectFirst("img")?.attr("src")
            MovieSearchResponse(
                title,
                href,
                this.name,
                TvType.Movie,
                poster = poster
            )
        }
    }
}