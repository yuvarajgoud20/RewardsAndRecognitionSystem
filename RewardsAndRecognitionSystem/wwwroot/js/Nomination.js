document.querySelectorAll("button.open-popup").forEach(btn => {
    btn.addEventListener("click", () => {
        const index = btn.dataset.index;
        const row = document.querySelector(`tr[data-index="${index}"]`);
        const data = row.dataset;
        const isReviewed = data.reviewed === "true";
        const isDirector = data.director === "true";

        const html = `
          <div class="popup-review-card">
    <!-- Sticky Header -->
    <div class="popup-sticky-header">
        <h4 class="text-indigo mb-3">
            <i class="bi bi-award-fill me-2"></i> Nomination ${isReviewed ? "Details" : "Review"}
        </h4>

        <div class="row mb-3 g-4">
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-person-fill me-1"></i>Nominee:</strong> <b>${data.name}</b></div>
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-diagram-3-fill me-1"></i>Team:</strong> <b>${data.team}</b></div>
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-tag-fill me-1"></i>Category:</strong> <b>${data.category}</b></div>
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-person-badge-fill me-1"></i>Nominated-By:</strong> <b>${data.nominatedby}</b></div>
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-info-circle-fill me-1"></i>Status:</strong> <b>${data.status}</b></div>
            <div class="col-md-4"><strong class="text-indigo"><i class="bi bi-calendar-event-fill me-1"></i>NominatedDate:</strong> <b>${data.created}</b></div>
        </div>
    </div>
    <!-- Scrollable Content Body -->
               <div class="popup-scroll-body">
            
                <div>
                       <label class="popup-label"><i class="bi bi-card-text me-1"></i><strong class="text-indigo">Description</strong></label>
                       <div id="descContent${index}" class="truncate-text">${data.description}</div>
                       <div id="descContent${index}-wrapper">
                       <span class="read-more-link" id="descContent${index}-toggle" onclick="toggleReadMore('descContent${index}')">Read more</span>
                       </div>
                </div>



               <div>
                  <label class="popup-label">
                     <i class="bi bi-stars me-1"></i><strong class="text-indigo">Achievements</strong>
                 </label>
                <div id="achContent${index}" class="truncate-text">${data.achievements}</div>
                <div id="achContent${index}-wrapper">
                  <span class="read-more-link" id="achContent${index}-toggle" onclick="toggleReadMore('achContent${index}')">Read more</span>
                </div>
               </div>

                
                ${isDirector && data.manager_remarks ? `
                  <div class="mb-3">
                     <label class="popup-label"><i class="bi bi-card-text me-1"></i><strong class="text-indigo">Manager Approval Status</strong></label>
                    <div class="">${data.approval_status}</div>
                  </div>
                 <div class="mb-3">
                     <label class="popup-label">
                     <i class="bi bi-card-text me-1"></i><strong class="text-indigo">Manager Remarks</strong>
                     </label>
                    <div id="mgrRemarks${index}" class="truncate-text">${data.manager_remarks}</div>
                    <div id="mgrRemarks${index}-wrapper">
                      <span class="read-more-link" id="mgrRemarks${index}-toggle" onclick="toggleReadMore('mgrRemarks${index}')">Read more</span>
                    </div>
                </div>
                ` : ``}


                <div class="mb-4">
                  <label class="popup-label">
                     <i class="bi bi-chat-left-text me-1"></i><strong class="text-indigo">Your Remarks</strong>
                  </label>
             ${isReviewed
                ? `
               <div id="yourRemarks${index}" class="truncate-text">${isDirector ? data.director_remarks : data.manager_remarks}</div>
                <div id="yourRemarks${index}-wrapper">
                <span class="read-more-link" id="yourRemarks${index}-toggle" onclick="toggleReadMore('yourRemarks${index}')">Read more</span>
                </div>
                   `
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

    </div>
            
        `;

        document.getElementById("popupContent").innerHTML = html;
        checkOverflowAndToggleLink(`descContent${index}`);
        checkOverflowAndToggleLink(`achContent${index}`);
        if (isDirector && data.manager_remarks) {
            checkOverflowAndToggleLink(`mgrRemarks${index}`);
        }

        if (isReviewed) {
            checkOverflowAndToggleLink(`yourRemarks${index}`);
        }
        document.getElementById("popupOverlay").style.display = "flex";
    });
});
function toggleReadMore(id) {
    const contentDiv = document.getElementById(id);
    const link = document.getElementById(id + "-toggle");

    if (contentDiv.classList.contains("truncate-text")) {
        contentDiv.classList.remove("truncate-text");
        link.innerText = "Show less";
    } else {
        contentDiv.classList.add("truncate-text");
        link.innerText = "Read more";
    }
}
function checkOverflowAndToggleLink(id) {
    const content = document.getElementById(id);
    const wrapper = document.getElementById(id + "-wrapper");

    // Check if content has overflowed (more height than visible area)
    const isOverflowing = content.scrollHeight > content.clientHeight;

    if (!isOverflowing) {
        wrapper.style.display = "none"; // Hide "Read more" if not needed
    }
}
function exportTableToExcel(tableId) {
    const table = document.getElementById(tableId);
    const clonedTable = table.cloneNode(true);

    // Remove rows that are hidden (i.e., filtered out)
    const rows = clonedTable.querySelectorAll("tbody tr");
    rows.forEach(row => {
        if (row.style.display === "none") {
            row.remove();
        }
    });

    // Remove action buttons column if desired
    clonedTable.querySelectorAll("td:last-child, th:last-child").forEach(cell => cell.remove());

    const wb = XLSX.utils.table_to_book(clonedTable, { sheet: "Nominations" });
    XLSX.writeFile(wb, "Nominations.xlsx");
}
function exportTableToExcelusers(tableId) {
    const table = document.getElementById(tableId);
    const clonedTable = table.cloneNode(true);

    // Remove rows that are hidden (i.e., filtered out)
    const rows = clonedTable.querySelectorAll("tbody tr");
    rows.forEach(row => {
        if (row.style.display === "none") {
            row.remove();
        }
    });

    // Remove action buttons column if desired
    clonedTable.querySelectorAll("td:last-child, th:last-child").forEach(cell => cell.remove());

    const wb = XLSX.utils.table_to_book(clonedTable, { sheet: "users" });
    XLSX.writeFile(wb, "users.xlsx");
}


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
        const status = (row.getAttribute("data-status") || "");
        let show = true;
        if (search && !name.includes(search)) show = false;
        if (filter === "pending" && !pending) show = false;
        if (filter === "reviewed" && !reviewed) show = false;
        if (filter === "directorapproved" && status != "DirectorApproved") show = false;
        if (filter === "directorrejected" && status != "DirectorRejected") show = false;
        

        row.style.display = show ? "" : "none";
    });
}
function filterTableUsers() {
    const search = document.getElementById("liveSearchUsers").value.toLowerCase();
    //const filter = document.getElementById("filterSelectUsers").value;

    document.querySelectorAll("#usersTable tbody tr").forEach(row => {
        const name = row.getAttribute("data-name") || "";
        //  const role = row.getAttribute("data-role") || "";
        let show = true;
        if (search && !name.includes(search)) show = false;
        // if (filter !== "all" && role !== filter.toLowerCase()) show = false;
        row.style.display = show ? "" : "none";
    });
}

document.getElementById("filterSelectUsers")?.addEventListener("change", filterTableUsers);
document.getElementById("liveSearchUsers")?.addEventListener("input", filterTableUsers);
document.getElementById("filterSelect")?.addEventListener("change", filterTable);
document.getElementById("liveSearch")?.addEventListener("input", filterTable);