document.querySelectorAll("button.open-popup").forEach(btn => {
    btn.addEventListener("click", () => {
        const index = btn.dataset.index;
        const row = document.querySelector(`tr[data-index="${index}"]`);
        const data = row.dataset;
        const isReviewed = data.reviewed === "true";
        const isDirector = data.director === "true";

        const html = `
            <div class="popup-review-card p-5">
                <h4 class="text-indigo mb-4"><i class="bi bi-award-fill me-2"></i> Nomination ${isReviewed ? "Details" : "Review"}</h4>

                <div class="row mb-4 g-4">
                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-person-fill me-1 text-indigo"></i>Nominee:</strong>  <b>${data.name}</b></div>
                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-diagram-3-fill me-1 text-indigo"></i>Team:</strong>  <b>${data.team}</b></div>
                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-tag-fill me-1 text-indigo"></i>Category:</strong>  <b>${data.category}</b></div>

                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-person-badge-fill me-1 text-indigo"></i>Nominated-By:</strong>  <b>${data.nominatedby}</b></div>
                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-info-circle-fill me-1 text-indigo"></i>Status:</strong>  <b>${data.status}</b></div>
                    <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-calendar-event-fill me-1 text-indigo"></i>NominatedDate:</strong>  <b>${data.created}</b></div>
                </div>

                <div class="mb-3">
                    <label class="popup-label"><i class="bi bi-card-text me-1"></i><strong class="text-indigo">Description</strong></label>
                    <div class="">${data.description}</div>
                </div>

                <div class="mb-3">
                    <label class="popup-label"><i class="bi bi-stars me-1"></i><strong class="text-indigo">Achievements</strong></label>
                    <div class="">${data.achievements}</div>
                </div>
                
                ${isDirector ? `
                    
                    <div class="mb-3">
                        <label class="popup-label"><i class="bi bi-card-text me-1"></i><strong class="text-indigo">Manager Approval Status</strong></label>
                        <div class="">${data.approval_status}</div>
                    </div>
                    <div class="mb-3">
                        <label class="popup-label"><i class="bi bi-card-text me-1"></i><strong class="text-indigo">Manager Remarks</strong></label>
                        <div class="">${data.manager_remarks}</div>
                    </div>
                `: ``}

                <div class="mb-4">
                    <label class="popup-label"><i class="bi bi-chat-left-text me-1"></i><strong class="text-indigo">Your Remarks</strong></label>
                    ${isReviewed
                ? `<div class="">${isDirector ? data.director_remarks : data.manager_remarks}</div>`
                : `<textarea id="popupRemarks" class="form-control" rows="3" placeholder="Add remarks..." required></textarea>`
            }
                </div>

                ${isReviewed
                ? ""
                : `
                        <div class="d-flex justify-content-end gap-3">
                            <button class="btn btn-success px-4" onclick="submitReview('${data.id}', 'Approved')">
                                <i class="bi bi-check-circle-fill"></i> Approve
                            </button>
                            <button class="btn btn-danger px-4" onclick="submitReview('${data.id}', 'Rejected')">
                                <i class="bi bi-x-circle-fill"></i> Reject
                            </button>
                        </div>
                    `
            }
            </div>
        `;

        document.getElementById("popupContent").innerHTML = html;
        document.getElementById("popupOverlay").style.display = "flex";
    });
});

function closePopup() {
    document.getElementById("popupOverlay").style.display = "none";
}

function submitReview(id, action) {
    const remarks = document.getElementById("popupRemarks").value;
    document.getElementById("reviewId").value = id;
    document.getElementById("reviewAction").value = action;
    document.getElementById("reviewRemarks").value = remarks;
    document.getElementById("reviewForm").submit();
}

document.getElementById('deleteModal')?.addEventListener('show.bs.modal', function (e) {
    const button = e.relatedTarget;
    document.getElementById('deleteNominationId').value = button.getAttribute('data-id');
});

function filterTable() {
    const search = document.getElementById("liveSearch").value.toLowerCase();
    const filter = document.getElementById("filterSelect").value;

    document.querySelectorAll("#nominationsTable tbody tr").forEach(row => {
        const name = row.getAttribute("data-name");
        const reviewed = row.classList.contains("reviewed");
        const pending = row.classList.contains("pending");
        const status = (row.getAttribute("data-status") || "").toLowerCase();
        let show = true;
        if (search && !name.includes(search)) show = false;
        if (filter === "pending" && !pending) show = false;
        if (filter === "reviewed" && !reviewed) show = false;
        if (filter === "approved" && status != "approved") show = false;
        if (filter === "rejected" && status != "rejected") show = false;

        row.style.display = show ? "" : "none";
    });
}

document.getElementById("filterSelect")?.addEventListener("change", filterTable);
document.getElementById("liveSearch")?.addEventListener("input", filterTable);
