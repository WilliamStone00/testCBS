(function () {
    const treeContainer = '#accountsTree';
    const refreshBtn = '#refreshTree';
    const expandBtn = '#expandAll';
    const collapseBtn = '#collapseAll';
    const searchInput = '#treeSearch';
    const languageToggle = '#languageToggle';
    const languageLabel = '#languageLabel';
    const spinner = '#treeSpinner';

    const noSelection = '#noSelection';
    const detailsPane = '#accountInfo';
    const formSelector = '#editAccountForm';
    const saveButton = '#saveButton';
    const successText = '#successText';
    const successModalEl = document.getElementById('successModal');

    const antiToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    function ajaxHeaders() { return antiToken ? { 'RequestVerificationToken': antiToken } : {}; }

    function showSpinner(show) { document.querySelector(spinner)?.classList.toggle('d-none', !show); }

    function debounce(fn, delay) { let t; return function () { clearTimeout(t); const args = arguments; t = setTimeout(() => fn.apply(this, args), delay); }; }

    function loadChartOfAccounts() {
        showSpinner(true);
        fetch(treeUrl, { method: 'GET', credentials: 'same-origin' })
            .then(r => r.json())
            .then(data => {
                showSpinner(false);
                if (!Array.isArray(data)) { alert('Failed to load chart of accounts.'); return; }
                if ($.jstree && $(treeContainer).data('jstree')) {
                    $(treeContainer).jstree(true).settings.core.data = data;
                    $(treeContainer).jstree(true).refresh();
                } else {
                    $(treeContainer).jstree({ core: { data: data, multiple: false, themes: { responsive: true } }, plugins: ['wholerow', 'search'] });
                    $(treeContainer).on('select_node.jstree', function (e, obj) { populateDetailsFromNode(obj.node); });
                }
            })
            .catch(err => { showSpinner(false); console.error(err); alert('Failed to load chart of accounts.'); });
    }

    function populateDetailsFromNode(node) {
        const o = node.original || {};
        const code = (node.text.split(' - ')[0] || '').trim();
        const selEl = document.querySelector('#accountId') || document.querySelector('#selectedID');
        if (selEl) selEl.value = node.id;
        document.querySelector('#accountCode')?.textContent = code;
        document.querySelector('#accountNameEn')?.value = o['data-name-en'] || o.Name || node.text;
        document.querySelector('#accountNameFr')?.value = o['data-name-fr'] || '';
        document.querySelector('#accountClass')?.textContent = o.Class || '';
        document.querySelector('#postingBadge')?.classList.toggle('d-none', !o.PostingAllowed);

        const meta = [];
        if (o.Path) meta.push('Path: ' + o.Path);
        if (o.Depth !== undefined) meta.push('Depth: ' + o.Depth);
        if (o.CreatedDate) meta.push('Created: ' + o.CreatedDate);
        document.querySelector('#accountMeta')?.textContent = meta.join(' • ');

        document.querySelector(noSelection).style.display = 'none';
        document.querySelector(detailsPane).style.display = 'block';
        document.querySelector(saveButton).disabled = false;
    }

    function updateAccountName(payload) {
        const headers = Object.assign({ 'Content-Type': 'application/json' }, ajaxHeaders());
        return fetch(updateUrl, { method: 'POST', headers, body: JSON.stringify(payload), credentials: 'same-origin' }).then(r => r.json());
    }

    document.addEventListener('submit', function (e) {
        if (!e.target.matches(formSelector)) return;
        e.preventDefault();
        const selectedEl = document.querySelector('#accountId') || document.querySelector('#selectedID');
        const id = selectedEl?.value;
        if (!id) { alert('No account selected.'); return; }

        const payload = { Id: id, NameEn: document.querySelector('#accountNameEn').value, NameFr: document.querySelector('#accountNameFr').value };
        updateAccountName(payload).then(resp => {
            if (resp?.success) {
                if ($.jstree && $(treeContainer).data('jstree')) {
                    const sel = $(treeContainer).jstree('get_selected', true)[0];
                    if (sel) $(treeContainer).jstree('rename_node', sel.id, sel.text.split(' - ')[0] + ' - ' + (payload.NameEn || payload.NameFr));
                }
                if (successModalEl) new bootstrap.Modal(successModalEl).show();
                document.querySelector(successText).textContent = resp.message || 'Account updated successfully';
            } else alert(resp.message || 'Update failed.');
        }).catch(err => { console.error(err); alert('Failed to update account.'); });
    });

    document.addEventListener('click', function (e) {
        const btn = e.target.closest('button');
        if (!btn) return;
        if (btn.matches(refreshBtn)) loadChartOfAccounts();
        else if (btn.matches(expandBtn) && $.jstree) $(treeContainer).jstree('open_all');
        else if (btn.matches(collapseBtn) && $.jstree) $(treeContainer).jstree('close_all');
    });

    const searchEl = document.querySelector(searchInput);
    if (searchEl) searchEl.addEventListener('input', debounce(() => { $.jstree && $(treeContainer).jstree(true).search(searchEl.value); }, 300));

    const langEl = document.querySelector(languageToggle);
    if (langEl) langEl.addEventListener('change', function () { document.querySelector(languageLabel).textContent = this.checked ? 'French' : 'English'; });

    $(function () { loadChartOfAccounts(); });
})();
