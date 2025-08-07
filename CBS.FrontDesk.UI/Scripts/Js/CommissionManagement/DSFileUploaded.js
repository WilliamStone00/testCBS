const API = {
    base: '/DailyAgentManagement', // Adjust if your API route is different
    endpoint: '/GetListOfUploadedFiles',
    headers: {
        'Content-Type': 'application/json',
    }
};

let allFiles = [];
let currentSort = { column: '', asc: true };

document.addEventListener('DOMContentLoaded', () => {
    loadFiles();
    document.getElementById('searchInput').addEventListener('input', renderFiles);
    document.getElementById('categoryFilter').addEventListener('change', renderFiles);
    document.querySelectorAll('.sortable').forEach(header =>
        header.addEventListener('click', () => sortTable(header.dataset.column))
    );
});

 


 
let currentPage = 1;
let itemsPerPage = 10; // Default items per page

function loadFiles() {
    $.ajax({
        url: `${API.base}${API.endpoint}`,
        headers: API.headers,
        success: function (response) {

            console.log(response.data);
            if (!Array.isArray(response.data)) {
                throw new Error("Unexpected response format: expected an array");
            }

            allFiles = response.data.map(x => ({
                fileName: x.fileName || x.FileName || 'Unnamed File',
                filePath: x.filePath || x.FilePath || '#',
                branchName: x.branchName || x.BranchName || 'Unknown',
                uploadedBy: x.uploadedBy || x.UploadedBy || 'N/A',
                uploadedOn: formatDate(x.uploadedOn || x.UploadedOn),
                fileCategory: x.fileCategory || x.FileCategory || 'Uncategorized'
            }));

            populateCategories();
            currentPage = 1; // Reset to first page when loading new data
            renderFiles();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('❌ Failed to load files:', errorThrown);

            // Optional: show user-friendly message on UI
            const tableBody = document.getElementById('tableBody');
            const emptyState = document.getElementById('emptyState');
            const resultsInfo = document.getElementById('resultsInfo');

            if (tableBody) tableBody.innerHTML = '';
            if (resultsInfo) resultsInfo.textContent = 'An error occurred while fetching file data.';
            if (emptyState) emptyState.style.display = 'block';
        }
    });
}

function populateCategories() {
    const categories = [...new Set(allFiles.map(f => f.fileCategory).filter(Boolean))];
    const filter = document.getElementById('categoryFilter');
    categories.forEach(cat => {
        const opt = document.createElement('option');
        opt.value = cat;
        opt.textContent = cat;
        filter.appendChild(opt);
    });
}

function renderFiles() {
    const searchText = document.getElementById('searchInput').value.toLowerCase();
    const selectedCategory = document.getElementById('categoryFilter').value;
    const tbody = document.getElementById('tableBody');
    const info = document.getElementById('resultsInfo');

    let files = [...allFiles];

    // Filter by search text
    if (searchText) {
        files = files.filter(f =>
            f.fileName.toLowerCase().includes(searchText) ||
            f.branchName.toLowerCase().includes(searchText) ||
            f.uploadedBy.toLowerCase().includes(searchText)
        );
    }

    // Filter by category
    if (selectedCategory) {
        files = files.filter(f => f.fileCategory === selectedCategory);
    }

    // Sort
    if (currentSort.column) {
        files.sort((a, b) => {
            const valA = a[currentSort.column] || '';
            const valB = b[currentSort.column] || '';
            return currentSort.asc ? valA.localeCompare(valB) : valB.localeCompare(valA);
        });
    }

    // Calculate pagination
    const totalItems = files.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const paginatedFiles = files.slice(startIndex, endIndex);

    tbody.innerHTML = '';

    if (files.length === 0) {
        document.getElementById('emptyState').style.display = 'block';
        info.textContent = 'No results found';
        renderPagination(0, 0);
        return;
    }

    document.getElementById('emptyState').style.display = 'none';

    // Update results info with pagination details
    const startItem = startIndex + 1;
    const endItem = Math.min(endIndex, totalItems);
    info.textContent = `Showing ${startItem}-${endItem} of ${totalItems} file(s)`;

    paginatedFiles.forEach(file => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
        <td>${file.fileName}</td>
        <td>${file.branchName}</td>
        <td>${file.uploadedBy}</td>
        <td>${file.uploadedOn}</td>
        <td>${file.fileCategory}</td>
        <td>
            <button class="btn btn-primary" onclick="viewFile('${API.base}/GetFileUploadApproval/${file.fileUploadId}')">👁️</button>
            <button class="btn btn-dark" onclick="downloadFile('${file.filePath}', '${file.fileName}')">⬇️</button>
        </td>`;
        tbody.appendChild(tr);
    });

    renderPagination(totalPages, totalItems);
}

function renderPagination(totalPages, totalItems) {
    const paginationContainer = document.getElementById('paginationContainer');

    if (!paginationContainer) {
        console.warn('Pagination container not found');
        return;
    }

    paginationContainer.innerHTML = '';

    if (totalPages <= 1) {
        return; // No pagination needed
    }

    const pagination = document.createElement('nav');
    pagination.setAttribute('aria-label', 'File pagination');

    const ul = document.createElement('ul');
    ul.className = 'pagination justify-content-center';

    // Previous button
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(${currentPage - 1}); return false;">Previous</a>`;
    ul.appendChild(prevLi);

    // Page numbers
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    // First page + ellipsis if needed
    if (startPage > 1) {
        const firstLi = document.createElement('li');
        firstLi.className = 'page-item';
        firstLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(1); return false;">1</a>`;
        ul.appendChild(firstLi);

        if (startPage > 2) {
            const ellipsisLi = document.createElement('li');
            ellipsisLi.className = 'page-item disabled';
            ellipsisLi.innerHTML = '<span class="page-link">...</span>';
            ul.appendChild(ellipsisLi);
        }
    }

    // Page numbers around current page
    for (let i = startPage; i <= endPage; i++) {
        const li = document.createElement('li');
        li.className = `page-item ${i === currentPage ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="changePage(${i}); return false;">${i}</a>`;
        ul.appendChild(li);
    }

    // Last page + ellipsis if needed
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            const ellipsisLi = document.createElement('li');
            ellipsisLi.className = 'page-item disabled';
            ellipsisLi.innerHTML = '<span class="page-link">...</span>';
            ul.appendChild(ellipsisLi);
        }

        const lastLi = document.createElement('li');
        lastLi.className = 'page-item';
        lastLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(${totalPages}); return false;">${totalPages}</a>`;
        ul.appendChild(lastLi);
    }

    // Next button
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(${currentPage + 1}); return false;">Next</a>`;
    ul.appendChild(nextLi);

    pagination.appendChild(ul);
    paginationContainer.appendChild(pagination);
}

function changePage(page) {
    if (page < 1) return;

    // Calculate total pages based on current filtered results
    const searchText = document.getElementById('searchInput').value.toLowerCase();
    const selectedCategory = document.getElementById('categoryFilter').value;

    let files = [...allFiles];

    if (searchText) {
        files = files.filter(f =>
            f.fileName.toLowerCase().includes(searchText) ||
            f.branchName.toLowerCase().includes(searchText) ||
            f.uploadedBy.toLowerCase().includes(searchText)
        );
    }

    if (selectedCategory) {
        files = files.filter(f => f.fileCategory === selectedCategory);
    }

    const totalPages = Math.ceil(files.length / itemsPerPage);

    if (page > totalPages) return;

    currentPage = page;
    renderFiles();
}

function changeItemsPerPage(newItemsPerPage) {
    itemsPerPage = parseInt(newItemsPerPage);
    currentPage = 1; // Reset to first page
    renderFiles();
}

// Helper function to reset pagination when filters change
function resetPagination() {
    currentPage = 1;
    renderFiles();
}

function sortTable(column) {
    if (currentSort.column === column) {
        currentSort.asc = !currentSort.asc;
    } else {
        currentSort.column = column;
        currentSort.asc = true;
    }
    renderFiles();
}
function formatDate(dateString) {
    if (!dateString) return '';

    let d;

    // Handle .NET JSON date format /Date(timestamp)/
    const dotNetDatePattern = /^\/Date\((\d+)\)\/$/;
    const dotNetMatch = dateString.match(dotNetDatePattern);

    if (dotNetMatch) {
        const timestamp = parseInt(dotNetMatch[1]);
        d = new Date(timestamp);
    }
    // Handle DD-MMM-YYYY format (like 31-jul-2025)
    else {
        const ddMmmYyyyPattern = /^(\d{1,2})-([a-zA-Z]{3})-(\d{4})$/i;
        const match = dateString.match(ddMmmYyyyPattern);

        if (match) {
            const [, day, monthName, year] = match;
            const monthMap = {
                'jan': 0, 'feb': 1, 'mar': 2, 'apr': 3, 'may': 4, 'jun': 5,
                'jul': 6, 'aug': 7, 'sep': 8, 'oct': 9, 'nov': 10, 'dec': 11
            };
            const month = monthMap[monthName.toLowerCase()];

            if (month !== undefined) {
                d = new Date(parseInt(year), month, parseInt(day));
            } else {
                return 'Invalid Date';
            }
        } else {
            // Fallback to standard Date constructor
            d = new Date(dateString);
        }
    }

    // Check if the date is valid
    if (isNaN(d.getTime())) {
        return 'Invalid Date';
    }

    return d.toLocaleDateString() + ' ' + d.toLocaleTimeString();
}
function viewFile(filePath) {
    //   if (!filePath) return alert("File path missing.");
    window.open(filePath, '_blank');
}

function downloadFile(filePath, fileName) {
    if (!filePath) return alert("No file path available");

    $.ajax({
        url: `${API.base}/${filePath}`,
        headers: API.headers,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (blob) {
            const url = URL.createObjectURL(blob);

            const a = document.createElement('a');
            a.href = url;
            a.download = fileName || 'downloaded-file';
            document.body.appendChild(a);
            a.click();

            URL.revokeObjectURL(url);
            $(a).remove();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error('❌ Download failed:', errorThrown);
            alert("Download failed.");
        }
    });
}
