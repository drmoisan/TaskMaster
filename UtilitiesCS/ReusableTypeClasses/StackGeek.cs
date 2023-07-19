using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS
{
    /* C# Program to implement a stack
     * that supports findMiddle()
     * and deleteMiddle in O(1) time */
    using System;

    class GFG
    {
        /* A Doubly Linked List Node */
        public class DLLNode
        {
            public DLLNode prev;
            public int data;
            public DLLNode next;
            public DLLNode(int d) { data = d; }
        }

        /* Representation of the stack
        data structure that supports
        findMiddle() in O(1) time. The
        Stack is implemented using
        Doubly Linked List. It maintains
        pointer to head node, pointer
        to middle node and count of
        nodes */
        public class myStack
        {
            public DLLNode head;
            public DLLNode mid;
            public int count;
        }

        /* Function to create the stack data structure */
        myStack createMyStack()
        {
            myStack ms = new myStack();
            ms.count = 0;
            return ms;
        }

        /* Function to push an element to the stack */
        void push(myStack ms, int new_data)
        {

            /* allocate DLLNode and put in data */
            DLLNode new_DLLNode = new DLLNode(new_data);

            /* Since we are adding at the beginning,
            prev is always NULL */
            new_DLLNode.prev = null;

            /* link the old list of the new DLLNode */
            new_DLLNode.next = ms.head;

            /* Increment count of items in stack */
            ms.count += 1;

            /* Change mid pointer in two cases
            1) Linked List is empty
            2) Number of nodes in linked list is odd */
            if (ms.count == 1)
            {
                ms.mid = new_DLLNode;
            }
            else
            {
                ms.head.prev = new_DLLNode;

                // Update mid if ms->count is odd
                if ((ms.count % 2) != 0)
                    ms.mid = ms.mid.prev;
            }

            /* move head to point to the new DLLNode */
            ms.head = new_DLLNode;
        }

        /* Function to pop an element from stack */
        int pop(myStack ms)
        {
            /* Stack underflow */
            if (ms.count == 0)
            {
                Console.WriteLine("Stack is empty");
                return -1;
            }

            DLLNode head = ms.head;
            int item = head.data;
            ms.head = head.next;

            // If linked list doesn't become empty,
            // update prev of new head as NULL
            if (ms.head != null)
                ms.head.prev = null;

            ms.count -= 1;

            // update the mid pointer when
            // we have even number of elements
            // in the stack, i,e move down
            // the mid pointer.
            if (ms.count % 2 == 0)
                ms.mid = ms.mid.next;

            return item;
        }

        // Function for finding middle of the stack
        int findMiddle(myStack ms)
        {
            if (ms.count == 0)
            {
                Console.WriteLine("Stack is empty now");
                return -1;
            }
            return ms.mid.data;
        }

        void deleteMiddle(myStack ms)
        {
            if (ms.count == 0)
            {
                Console.WriteLine("Stack is empty now");
                return;
            }

            ms.count -= 1;
            ms.mid.next.prev = ms.mid.prev;
            ms.mid.prev.next = ms.mid.next;

            if (ms.count % 2 != 0)
            {
                ms.mid = ms.mid.next;
            }
            else
            {
                ms.mid = ms.mid.prev;
            }

        }

        // Driver code
        public static void Main(String[] args)
        {
            GFG ob = new GFG();
            myStack ms = ob.createMyStack();
            ob.push(ms, 11);
            ob.push(ms, 22);
            ob.push(ms, 33);
            ob.push(ms, 44);
            ob.push(ms, 55);
            ob.push(ms, 66);
            ob.push(ms, 77);
            ob.push(ms, 88);
            ob.push(ms, 99);

            Console.WriteLine("Popped : " + ob.pop(ms));
            Console.WriteLine("Popped : " + ob.pop(ms));
            Console.WriteLine("Middle Element : "
                            + ob.findMiddle(ms));
            ob.deleteMiddle(ms);
            Console.WriteLine("New Middle Element : "
                                + ob.findMiddle(ms));
        }
    }

    // This code is contributed
    // by Arnab Kundu

    // Updated by Amsavarthan Lv

}
