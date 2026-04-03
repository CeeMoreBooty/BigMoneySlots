package com.ceemoorebooty.idledozer

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.Button
import android.widget.TextView
import androidx.recyclerview.widget.RecyclerView

/**
 * Displays the list of dozer types available to purchase.
 * Buttons are visually disabled when the player cannot afford the next unit.
 */
class DozerAdapter(
    private val dozers: List<DozerItem>,
    private val onBuy: (DozerItem) -> Unit
) : RecyclerView.Adapter<DozerAdapter.ViewHolder>() {

    /** Updated by MainActivity before each adapter refresh so affordability is shown correctly. */
    var currentBalance: Double = 0.0

    class ViewHolder(view: View) : RecyclerView.ViewHolder(view) {
        val tvName: TextView  = view.findViewById(R.id.tv_dozer_name)
        val tvOwned: TextView = view.findViewById(R.id.tv_dozer_owned)
        val tvEps: TextView   = view.findViewById(R.id.tv_dozer_eps)
        val btnBuy: Button    = view.findViewById(R.id.btn_buy_dozer)
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        val view = LayoutInflater.from(parent.context)
            .inflate(R.layout.item_dozer, parent, false)
        return ViewHolder(view)
    }

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val dozer = dozers[position]
        holder.tvName.text  = "${dozer.emoji} ${dozer.name}"
        holder.tvOwned.text = "Owned: ${dozer.owned}"
        holder.tvEps.text   = "Each earns \$${MainActivity.formatMoney(dozer.baseEarningsPerSecond)}/sec"
        holder.btnBuy.text  = "Buy \$${MainActivity.formatMoney(dozer.currentCost)}"
        holder.btnBuy.alpha = if (currentBalance >= dozer.currentCost) 1.0f else 0.4f
        holder.btnBuy.setOnClickListener { onBuy(dozer) }
    }

    override fun getItemCount(): Int = dozers.size
}
