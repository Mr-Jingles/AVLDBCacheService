namespace AVLDBCacheService
{
    public class CacheEntry
    {
        public CacheEntry left;
        public CacheEntry right;

        public string sql;
        public string dbname;
        public string uniqueIdentifier;
        public DateTime expiryTime;
        public string result;

        public CacheEntry(string sql, string dbname, DateTime expiryTime, string result)
        {
            this.sql = sql;
            this.dbname = dbname;
            this.uniqueIdentifier = sql + dbname;
            this.expiryTime = expiryTime;
            this.result = result;
        }
    }

    public class AVLTree
    {
       
        CacheEntry topLevel;
        public AVLTree()
        {
            //Do nothing for now
        }

        public void Insert(string sql, string dbname, DateTime expiryTime, string result)
        {
            CacheEntry newItem = new CacheEntry(sql, dbname, expiryTime, result);
            //if we dont have a top level add one now
            if (topLevel == null)
            {
                topLevel = newItem;
            }
            else
            {
                //recurse to add new item
                topLevel = RecursiveInsert(topLevel, newItem);
            }
        }
        private CacheEntry RecursiveInsert(CacheEntry current, CacheEntry node)
        {
            if (current == null)
            {
                current = node;
                return current;
            }

            var compare = String.Compare(node.uniqueIdentifier, current.uniqueIdentifier);

            if (compare == 1)
            {
                current.left = RecursiveInsert(current.left, node);
                current = BalanceTree(current);
            }
            else if (compare == -1)
            {
                current.right = RecursiveInsert(current.right, node);
                current = BalanceTree(current);
            }

            return current;
        }
        private CacheEntry BalanceTree(CacheEntry current)
        {
            int b_factor = balance_factor(current);
            if (b_factor > 1)
            {
                if (balance_factor(current.left) > 0)
                {
                    current = RotateLeftLeft(current);
                }
                else
                {
                    current = RotateLeftRight(current);
                }
            }
            else if (b_factor < -1)
            {
                if (balance_factor(current.right) > 0)
                {
                    current = RotateRightLeft(current);
                }
                else
                {
                    current = RotateRightRight(current);
                }
            }
            return current;
        }
        public void Delete(string target)
        {
            //traverse the tree to delete
            topLevel = Delete(topLevel, target);
        }
        private CacheEntry Delete(CacheEntry current, string target)
        {
            CacheEntry parent;
            if (current == null)
            { return null; }
            else
            {
                var compare = String.Compare(target, current.uniqueIdentifier);

                //left subtree
                if (compare == -1)
                {
                    current.left = Delete(current.left, target);
                    if (balance_factor(current) == -2)//here
                    {
                        if (balance_factor(current.right) <= 0)
                        {
                            current = RotateRightRight(current);
                        }
                        else
                        {
                            current = RotateRightLeft(current);
                        }
                    }
                }
                //right subtree
                else if (compare == 1)
                {
                    current.right = Delete(current.right, target);
                    if (balance_factor(current) == 2)
                    {
                        if (balance_factor(current.left) >= 0)
                        {
                            current = RotateLeftLeft(current);
                        }
                        else
                        {
                            current = RotateLeftRight(current);
                        }
                    }
                }
                //if target is found
                else
                {
                    if (current.right != null)
                    {
                        //delete its inorder successor
                        parent = current.right;
                        while (parent.left != null)
                        {
                            parent = parent.left;
                        }

                        current.uniqueIdentifier = parent.uniqueIdentifier;
                        current.sql = parent.sql;
                        current.expiryTime = parent.expiryTime;
                        current.result = parent.result;

                        current.right = Delete(current.right, parent.uniqueIdentifier);
                        if (balance_factor(current) == 2)//rebalancing
                        {
                            if (balance_factor(current.left) >= 0)
                            {
                                current = RotateLeftLeft(current);
                            }
                            else { current = RotateLeftRight(current); }
                        }
                    }
                    else
                    {   //if current.left != null
                        return current.left;
                    }
                }
            }
            return current;
        }
        public CacheEntry Find(string uniqueIdentifier)
        {
            var result = Find(uniqueIdentifier, topLevel);
            if (result.uniqueIdentifier == uniqueIdentifier)
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        private CacheEntry Find(string target, CacheEntry current)
        {
            var compare = String.Compare(target, current.uniqueIdentifier);

            if (compare == -1)
            {
                if (target == current.uniqueIdentifier)
                {
                    return current;
                }
                else
                    return Find(target, current.left);
            }
            else
            {
                if (target == current.uniqueIdentifier)
                {
                    return current;
                }
                else
                    return Find(target, current.right);
            }

        }

        private int max(int l, int r)
        {
            return l > r ? l : r;
        }
        private int getHeight(CacheEntry current)
        {
            int height = 0;
            if (current != null)
            {
                int l = getHeight(current.left);
                int r = getHeight(current.right);
                int m = max(l, r);
                height = m + 1;
            }
            return height;
        }
        private int balance_factor(CacheEntry current)
        {
            int l = getHeight(current.left);
            int r = getHeight(current.right);
            int b_factor = l - r;
            return b_factor;
        }
        private CacheEntry RotateRightRight(CacheEntry parent)
        {
            CacheEntry pivot = parent.right;
            parent.right = pivot.left;
            pivot.left = parent;
            return pivot;
        }
        private CacheEntry RotateLeftLeft(CacheEntry parent)
        {
            CacheEntry pivot = parent.left;
            parent.left = pivot.right;
            pivot.right = parent;
            return pivot;
        }
        private CacheEntry RotateLeftRight(CacheEntry parent)
        {
            CacheEntry pivot = parent.left;
            parent.left = RotateRightRight(pivot);
            return RotateLeftLeft(parent);
        }
        private CacheEntry RotateRightLeft(CacheEntry parent)
        {
            CacheEntry pivot = parent.right;
            parent.right = RotateLeftLeft(pivot);
            return RotateRightRight(parent);
        }
    }
}
