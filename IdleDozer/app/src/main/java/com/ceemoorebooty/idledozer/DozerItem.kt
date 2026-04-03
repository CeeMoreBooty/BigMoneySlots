package com.ceemoorebooty.idledozer

/**
 * Represents a single type of dozer available for purchase in the shop.
 *
 * @param id                    Unique identifier used for save/load.
 * @param name                  Display name shown in the shop.
 * @param emoji                 Emoji icon shown next to the name.
 * @param baseCost              Initial purchase price before the cost-scaling formula is applied.
 * @param baseEarningsPerSecond How much each owned unit of this dozer earns per second.
 */
data class DozerItem(
    val id: Int,
    val name: String,
    val emoji: String,
    val baseCost: Double,
    val baseEarningsPerSecond: Double,
    var owned: Int = 0
) {
    /** Cost of the next purchase — increases by 15 % with every unit bought. */
    val currentCost: Double
        get() = baseCost * Math.pow(1.15, owned.toDouble())

    /** Combined earnings per second from all owned units of this type. */
    val totalEarningsPerSecond: Double
        get() = baseEarningsPerSecond * owned
}
